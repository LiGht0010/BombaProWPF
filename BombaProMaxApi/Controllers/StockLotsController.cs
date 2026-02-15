using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using BombaProMaxApi.Services;
using System.Text.Json;

namespace BombaProMaxApi.Controllers;

/// <summary>
/// Controller for StockLot management, opening balance, withdrawal, and margin reporting.
/// StockLots are created via Achat allocations or Opening Balance for initial setup.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StockLotsController : ControllerBase
{
    private readonly IStockLotService _stockLotService;
    private readonly AppDbContext _context;
    private readonly ILogger<StockLotsController> _logger;

    public StockLotsController(
        IStockLotService stockLotService, 
        AppDbContext context,
        ILogger<StockLotsController> logger)
    {
        _stockLotService = stockLotService;
        _context = context;
        _logger = logger;
    }

    // ???????????????????????????????????????????????????????????????????????
    // STOCK WITHDRAWAL ENDPOINTS (Super Admin Only)
    // ???????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Withdraws stock from a reservoir using FIFO order.
    /// Creates an audit trail record for the withdrawal.
    /// Super Admin only operation.
    /// </summary>
    /// <param name="dto">Withdrawal request details</param>
    /// <returns>Withdrawal result with affected lots breakdown</returns>
    [HttpPost("withdraw")]
    public async Task<ActionResult<StockWithdrawalResponseDto>> WithdrawStock(
        [FromBody] StockWithdrawalRequestDto dto)
    {
        _logger.LogInformation(
            "Stock withdrawal requested: Reservoir {ReservoirId}, Quantite {Quantite}L, User {UserId}",
            dto.ReservoirID, dto.Quantite, dto.UtilisateurID);

        if (dto.Quantite <= 0)
        {
            return BadRequest(new StockWithdrawalResponseDto
            {
                Success = false,
                Message = "La quantité doit être supérieure à zéro"
            });
        }

        // Use execution strategy for transaction support
        var strategy = _context.Database.CreateExecutionStrategy();
        
        StockWithdrawalResponseDto? result = null;
        string? errorMessage = null;

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Get reservoir with product info
                    var reservoir = await _context.Reservoirs
                        .Include(r => r.Produit)
                        .FirstOrDefaultAsync(r => r.ID == dto.ReservoirID);

                    if (reservoir == null)
                    {
                        errorMessage = $"Réservoir {dto.ReservoirID} non trouvé";
                        return;
                    }

                    var niveauAvant = reservoir.NiveauDeCarburant;

                    // Get available stock lots in FIFO order
                    var availableLots = await _context.StockLots
                        .Where(s => s.ReservoirID == dto.ReservoirID
                                 && s.Statut == "Disponible"
                                 && s.QuantiteDisponible > 0)
                        .OrderBy(s => s.DateEntree)
                        .ThenBy(s => s.ID)
                        .ToListAsync();

                    if (!availableLots.Any())
                    {
                        errorMessage = "Aucun stock disponible dans ce réservoir";
                        return;
                    }

                    var totalAvailable = availableLots.Sum(l => l.QuantiteDisponible);
                    if (totalAvailable < dto.Quantite)
                    {
                        errorMessage = $"Stock insuffisant: requis {dto.Quantite:N2}L, disponible {totalAvailable:N2}L";
                        return;
                    }

                    // Perform FIFO withdrawal
                    var remainingToWithdraw = dto.Quantite;
                    var affectedLots = new List<StockLotWithdrawalDetailDto>();

                    foreach (var lot in availableLots)
                    {
                        if (remainingToWithdraw <= 0)
                            break;

                        var quantiteAvant = lot.QuantiteDisponible;
                        var toWithdraw = Math.Min(lot.QuantiteDisponible, remainingToWithdraw);

                        // Update lot
                        lot.QuantiteDisponible -= toWithdraw;
                        var estEpuise = lot.QuantiteDisponible <= 0;
                        if (estEpuise)
                        {
                            lot.Statut = "Épuisé";
                        }

                        affectedLots.Add(new StockLotWithdrawalDetailDto
                        {
                            StockLotID = lot.ID,
                            QuantiteAvant = quantiteAvant,
                            QuantiteRetiree = toWithdraw,
                            QuantiteApres = lot.QuantiteDisponible,
                            PrixAchat = lot.PrixAchat,
                            Statut = lot.Statut,
                            EstEpuise = estEpuise
                        });

                        remainingToWithdraw -= toWithdraw;

                        _logger.LogDebug(
                            "Withdrew {Quantite}L from StockLot {LotId}, Remaining: {Remaining}L",
                            toWithdraw, lot.ID, lot.QuantiteDisponible);
                    }

                    // Update reservoir level
                    reservoir.NiveauDeCarburant -= dto.Quantite;
                    reservoir.DateModification = DateTime.UtcNow;

                    // Create audit record
                    var withdrawal = new StockWithdrawal
                    {
                        ReservoirID = dto.ReservoirID,
                        ProduitID = reservoir.ProduitID ?? 0,
                        Quantite = dto.Quantite,
                        Motif = dto.Motif,
                        UtilisateurID = dto.UtilisateurID,
                        UtilisateurNom = dto.UtilisateurNom,
                        DateRetrait = dto.DateRetrait.HasValue 
                            ? DateTime.SpecifyKind(dto.DateRetrait.Value, DateTimeKind.Utc) 
                            : DateTime.UtcNow,
                        NiveauAvant = niveauAvant,
                        NiveauApres = reservoir.NiveauDeCarburant,
                        LotsAffectesJson = JsonSerializer.Serialize(affectedLots)
                    };

                    _context.StockWithdrawals.Add(withdrawal);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    result = new StockWithdrawalResponseDto
                    {
                        Success = true,
                        Message = $"Retrait de {dto.Quantite:N2}L effectué avec succès",
                        ReservoirID = dto.ReservoirID,
                        ReservoirNumero = reservoir.Numero,
                        QuantiteRetiree = dto.Quantite,
                        NouveauNiveau = reservoir.NiveauDeCarburant,
                        DateRetrait = withdrawal.DateRetrait,
                        LotsAffectes = affectedLots
                    };

                    _logger.LogInformation(
                        "Stock withdrawal completed: {Quantite}L from Reservoir {ReservoirId}, " +
                        "{LotCount} lots affected, New level: {NewLevel}L",
                        dto.Quantite, dto.ReservoirID, affectedLots.Count, reservoir.NiveauDeCarburant);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock withdrawal");
            return StatusCode(500, new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur lors du retrait: {ex.Message}"
            });
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            return BadRequest(new StockWithdrawalResponseDto
            {
                Success = false,
                Message = errorMessage
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets withdrawal history for all reservoirs or a specific reservoir.
    /// </summary>
    /// <param name="reservoirId">Optional reservoir ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of withdrawal history records</returns>
    [HttpGet("withdrawals")]
    public async Task<ActionResult<List<StockWithdrawalHistoryDto>>> GetWithdrawalHistory(
        [FromQuery] int? reservoirId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.StockWithdrawals
            .Include(w => w.Reservoir)
            .Include(w => w.Produit)
            .AsNoTracking();

        if (reservoirId.HasValue)
            query = query.Where(w => w.ReservoirID == reservoirId.Value);

        if (startDate.HasValue)
        {
            var start = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(w => w.DateRetrait >= start);
        }

        if (endDate.HasValue)
        {
            var end = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            query = query.Where(w => w.DateRetrait <= end);
        }

        var withdrawals = await query
            .OrderByDescending(w => w.DateRetrait)
            .Select(w => new StockWithdrawalHistoryDto
            {
                ID = w.ID,
                ReservoirID = w.ReservoirID,
                ReservoirNumero = w.Reservoir.Numero,
                ProduitID = w.ProduitID,
                ProduitNom = w.Produit.Description,
                Quantite = w.Quantite,
                Motif = w.Motif,
                UtilisateurID = w.UtilisateurID,
                UtilisateurNom = w.UtilisateurNom,
                DateRetrait = w.DateRetrait
            })
            .ToListAsync();

        return Ok(withdrawals);
    }

    /// <summary>
    /// Deletes a withdrawal record and restores the stock to the affected lots.
    /// Reverses the FIFO withdrawal operation.
    /// </summary>
    /// <param name="id">Withdrawal ID to delete</param>
    /// <returns>Result with restoration details</returns>
    [HttpDelete("withdrawals/{id}")]
    public async Task<ActionResult<StockWithdrawalResponseDto>> DeleteWithdrawal(int id)
    {
        _logger.LogInformation("Delete withdrawal requested: ID {WithdrawalId}", id);

        // Use execution strategy for transaction support
        var strategy = _context.Database.CreateExecutionStrategy();
        
        StockWithdrawalResponseDto? result = null;
        string? errorMessage = null;

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Get the withdrawal record
                    var withdrawal = await _context.StockWithdrawals
                        .Include(w => w.Reservoir)
                        .FirstOrDefaultAsync(w => w.ID == id);

                    if (withdrawal == null)
                    {
                        errorMessage = $"Retrait {id} non trouvé";
                        return;
                    }

                    // Parse the affected lots from JSON
                    var affectedLots = new List<StockLotWithdrawalDetailDto>();
                    if (!string.IsNullOrEmpty(withdrawal.LotsAffectesJson))
                    {
                        affectedLots = JsonSerializer.Deserialize<List<StockLotWithdrawalDetailDto>>(
                            withdrawal.LotsAffectesJson) ?? [];
                    }

                    // Restore stock to each affected lot (reverse FIFO)
                    foreach (var lotDetail in affectedLots)
                    {
                        var stockLot = await _context.StockLots.FindAsync(lotDetail.StockLotID);
                        if (stockLot != null)
                        {
                            // Restore the withdrawn quantity
                            stockLot.QuantiteDisponible += lotDetail.QuantiteRetiree;
                            
                            // If lot was marked as "Épuisé", restore to "Disponible"
                            if (stockLot.Statut == "Épuisé" && stockLot.QuantiteDisponible > 0)
                            {
                                stockLot.Statut = "Disponible";
                            }

                            _logger.LogDebug(
                                "Restored {Quantite}L to StockLot {LotId}, New Qty: {NewQty}L",
                                lotDetail.QuantiteRetiree, lotDetail.StockLotID, stockLot.QuantiteDisponible);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "StockLot {LotId} not found during withdrawal reversal", 
                                lotDetail.StockLotID);
                        }
                    }

                    // Restore reservoir level
                    var reservoir = withdrawal.Reservoir;
                    if (reservoir != null)
                    {
                        reservoir.NiveauDeCarburant += withdrawal.Quantite;
                        reservoir.DateModification = DateTime.UtcNow;

                        _logger.LogDebug(
                            "Restored Reservoir {ReservoirId} level: {OldLevel}L -> {NewLevel}L",
                            reservoir.ID, withdrawal.NiveauApres, reservoir.NiveauDeCarburant);
                    }

                    // Delete the withdrawal record
                    _context.StockWithdrawals.Remove(withdrawal);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    result = new StockWithdrawalResponseDto
                    {
                        Success = true,
                        Message = $"Retrait annulé avec succès. {withdrawal.Quantite:N2}L restaurés.",
                        ReservoirID = withdrawal.ReservoirID,
                        ReservoirNumero = reservoir?.Numero,
                        QuantiteRetiree = withdrawal.Quantite,
                        NouveauNiveau = reservoir?.NiveauDeCarburant ?? 0,
                        DateRetrait = withdrawal.DateRetrait,
                        LotsAffectes = affectedLots
                    };

                    _logger.LogInformation(
                        "Withdrawal {WithdrawalId} deleted successfully. Restored {Quantite}L to Reservoir {ReservoirId}",
                        id, withdrawal.Quantite, withdrawal.ReservoirID);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during withdrawal deletion");
            return StatusCode(500, new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur lors de l'annulation: {ex.Message}"
            });
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            return NotFound(new StockWithdrawalResponseDto
            {
                Success = false,
                Message = errorMessage
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets reservoirs available for withdrawal with stock info.
    /// </summary>
    /// <returns>List of reservoirs with available stock</returns>
    [HttpGet("reservoirs-for-withdrawal")]
    public async Task<ActionResult<List<ReservoirWithdrawalInfoDto>>> GetReservoirsForWithdrawal()
    {
        var reservoirs = await _context.Reservoirs
            .Include(r => r.Produit)
            .Include(r => r.StockLots.Where(s => s.Statut == "Disponible"))
            .AsNoTracking()
            .ToListAsync();

        var result = reservoirs.Select(r => new ReservoirWithdrawalInfoDto
        {
            ID = r.ID,
            Numero = r.Numero,
            ProduitID = r.ProduitID,
            ProduitNom = r.Produit?.Description,
            Capacite = r.Capacite,
            NiveauActuel = r.NiveauDeCarburant,
            StockDisponible = r.StockLots.Sum(s => s.QuantiteDisponible),
            NombreLots = r.StockLots.Count(s => s.QuantiteDisponible > 0)
        })
        .OrderBy(r => r.Numero)
        .ToList();

        return Ok(result);
    }

    // ???????????????????????????????????????????????????????????????????????
    // OPENING BALANCE ENDPOINTS
    // ???????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Creates an Opening Balance StockLot for a reservoir.
    /// Used during onboarding when a reservoir has existing fuel.
    /// Only one Opening Balance is allowed per reservoir, and the reservoir must be empty.
    /// </summary>
    /// <param name="dto">Opening balance details</param>
    /// <returns>Created opening balance information</returns>
    [HttpPost("opening-balance")]
    public async Task<ActionResult<OpeningBalanceResultDto>> CreateOpeningBalance(
        [FromBody] OpeningBalanceCreateDto dto)
    {
        _logger.LogInformation(
            "Creating opening balance for Reservoir {ReservoirId}: {Quantite}L at {PrixAchat}/L",
            dto.ReservoirID, dto.Quantite, dto.PrixAchat);

        try
        {
            // Get reservoir info for response
            var reservoir = await _context.Reservoirs
                .Include(r => r.Produit)
                .FirstOrDefaultAsync(r => r.ID == dto.ReservoirID);

            if (reservoir == null)
            {
                return NotFound($"Reservoir {dto.ReservoirID} not found");
            }

            // Get product info
            var produit = await _context.Produits.FindAsync(dto.ProduitID);
            if (produit == null)
            {
                return BadRequest($"Produit {dto.ProduitID} not found");
            }

            // Create the opening balance
            var stockLotId = await _stockLotService.CreateOpeningBalanceAsync(
                dto.ReservoirID,
                dto.ProduitID,
                dto.Quantite,
                dto.PrixAchat,
                dto.DateEntree,
                dto.Notes);

            // Reload reservoir to get updated level
            await _context.Entry(reservoir).ReloadAsync();

            var result = new OpeningBalanceResultDto
            {
                StockLotID = stockLotId,
                ReservoirID = dto.ReservoirID,
                ReservoirNumero = reservoir.Numero,
                ProduitID = dto.ProduitID,
                ProduitNom = produit.Description,
                Quantite = dto.Quantite,
                PrixAchat = dto.PrixAchat,
                DateEntree = dto.DateEntree ?? DateTime.UtcNow,
                Notes = dto.Notes,
                NouveauNiveau = reservoir.NiveauDeCarburant,
                Message = $"Stock initial de {dto.Quantite:N2}L créé pour le réservoir {reservoir.Numero}"
            };

            _logger.LogInformation(
                "Opening balance created successfully: StockLot {StockLotId} for Reservoir {ReservoirId}",
                stockLotId, dto.ReservoirID);

            return CreatedAtAction(
                nameof(GetStockLotAnalysis), 
                new { id = stockLotId }, 
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create opening balance: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the stock status for a reservoir to determine if opening balance can be created.
    /// </summary>
    /// <param name="reservoirId">Reservoir ID</param>
    /// <returns>Status information including whether opening balance can be created</returns>
    [HttpGet("reservoir/{reservoirId}/status")]
    public async Task<ActionResult<ReservoirStockStatusDto>> GetReservoirStockStatus(int reservoirId)
    {
        var reservoir = await _context.Reservoirs
            .Include(r => r.Produit)
            .FirstOrDefaultAsync(r => r.ID == reservoirId);

        if (reservoir == null)
        {
            return NotFound($"Reservoir {reservoirId} not found");
        }

        var hasStockLots = await _stockLotService.HasAnyStockLotsAsync(reservoirId);
        var hasOpeningBalance = await _stockLotService.HasOpeningBalanceAsync(reservoirId);

        var status = new ReservoirStockStatusDto
        {
            ReservoirID = reservoir.ID,
            ReservoirNumero = reservoir.Numero,
            ProduitID = reservoir.ProduitID,
            ProduitNom = reservoir.Produit?.Description,
            Capacite = reservoir.Capacite,
            NiveauActuel = reservoir.NiveauDeCarburant,
            HasStockLots = hasStockLots,
            HasOpeningBalance = hasOpeningBalance,
            BlockingReason = hasStockLots 
                ? "Le réservoir possède déjà des lots de stock. Le stock initial ne peut être créé que pour un réservoir vide."
                : null
        };

        return Ok(status);
    }

    /// <summary>
    /// Validates if stock is available for a sale.
    /// </summary>
    /// <param name="reservoirId">Reservoir ID</param>
    /// <param name="produitId">Product ID</param>
    /// <param name="quantite">Required quantity</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate-availability")]
    public async Task<ActionResult<object>> ValidateStockAvailability(
        [FromQuery] int reservoirId,
        [FromQuery] int produitId,
        [FromQuery] decimal quantite)
    {
        var isAvailable = await _stockLotService.ValidateStockAvailabilityAsync(
            reservoirId, produitId, quantite);
        
        var available = await _stockLotService.GetAvailableStockAsync(reservoirId);

        return Ok(new
        {
            ReservoirID = reservoirId,
            ProduitID = produitId,
            QuantiteRequise = quantite,
            QuantiteDisponible = available,
            IsAvailable = isAvailable,
            Message = isAvailable 
                ? "Stock suffisant" 
                : $"Stock insuffisant: requis {quantite:N2}L, disponible {available:N2}L"
        });
    }

    // ?????????????????????????????????????????????????????????????????
    // ANALYSIS ENDPOINTS
    // ?????????????????????????????????????????????????????????????????

    /// <summary>
    /// Gets detailed margin analysis for a single StockLot.
    /// </summary>
    /// <param name="id">StockLot ID</param>
    /// <returns>StockLot analysis with margin data</returns>
    [HttpGet("{id}/analysis")]
    public async Task<ActionResult<StockLotAnalysisDto>> GetStockLotAnalysis(int id)
    {
        _logger.LogInformation("Getting analysis for StockLot {StockLotId}", id);

        var analysis = await _stockLotService.GetStockLotAnalysisAsync(id);
        
        if (analysis == null)
            return NotFound($"StockLot {id} not found");

        return Ok(analysis);
    }

    /// <summary>
    /// Gets aggregated margin analysis for all StockLots in a reservoir.
    /// </summary>
    /// <param name="reservoirId">Reservoir ID</param>
    /// <returns>Reservoir analysis with all StockLots and margin data</returns>
    [HttpGet("reservoir/{reservoirId}/analysis")]
    public async Task<ActionResult<ReservoirAnalysisDto>> GetReservoirAnalysis(int reservoirId)
    {
        _logger.LogInformation("Getting analysis for Reservoir {ReservoirId}", reservoirId);

        var analysis = await _stockLotService.GetReservoirAnalysisAsync(reservoirId);
        
        if (analysis == null)
            return NotFound($"Reservoir {reservoirId} not found");

        return Ok(analysis);
    }

    /// <summary>
    /// Gets global margin analysis summary across all reservoirs.
    /// Optionally filter by date range.
    /// </summary>
    /// <param name="startDate">Optional start date for filtering consumptions</param>
    /// <param name="endDate">Optional end date for filtering consumptions</param>
    /// <returns>Global analysis summary with per-product and per-reservoir breakdowns</returns>
    [HttpGet("analysis/summary")]
    public async Task<ActionResult<GlobalAnalysisSummaryDto>> GetGlobalAnalysis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        _logger.LogInformation(
            "Getting global analysis: StartDate={StartDate}, EndDate={EndDate}",
            startDate, endDate);

        // Ensure dates are UTC if provided
        if (startDate.HasValue)
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
        if (endDate.HasValue)
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

        var analysis = await _stockLotService.GetGlobalAnalysisAsync(startDate, endDate);
        
        return Ok(analysis);
    }

    /// <summary>
    /// Gets available stock quantity for a reservoir.
    /// </summary>
    /// <param name="reservoirId">Reservoir ID</param>
    /// <returns>Available stock in liters</returns>
    [HttpGet("reservoir/{reservoirId}/available")]
    public async Task<ActionResult<object>> GetAvailableStock(int reservoirId)
    {
        var available = await _stockLotService.GetAvailableStockAsync(reservoirId);
        
        return Ok(new 
        { 
            ReservoirID = reservoirId,
            QuantiteDisponible = available,
            Unite = "L"
        });
    }

    /// <summary>
    /// Gets margin analysis for a specific Periode.
    /// Shows which StockLots were consumed, at what PrixAchat, and calculates margins.
    /// </summary>
    /// <param name="periodeId">Periode ID</param>
    /// <returns>Detailed margin breakdown with FIFO cost tracking</returns>
    [HttpGet("periode/{periodeId}/marge")]
    public async Task<ActionResult<PeriodeMargeAnalysisDto>> GetPeriodeMargeAnalysis(int periodeId)
    {
        _logger.LogInformation("Getting marge analysis for Periode {PeriodeId}", periodeId);

        var analysis = await _stockLotService.GetPeriodeMargeAnalysisAsync(periodeId);
        
        if (analysis == null)
            return NotFound($"Periode {periodeId} not found");

        return Ok(analysis);
    }
}
