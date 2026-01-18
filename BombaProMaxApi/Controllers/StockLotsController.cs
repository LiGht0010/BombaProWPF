using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Services;

namespace BombaProMaxApi.Controllers;

/// <summary>
/// Controller for StockLot management, opening balance, and margin reporting.
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

    // ?????????????????????????????????????????????????????????????????
    // OPENING BALANCE ENDPOINTS
    // ?????????????????????????????????????????????????????????????????

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
