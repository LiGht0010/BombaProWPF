using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Services;

/// <summary>
/// FIFO stock lot consumption service.
/// Handles stock deduction during Periode creation and stock creation from purchases.
/// </summary>
public class StockLotService : IStockLotService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockLotService> _logger;

    public StockLotService(AppDbContext context, ILogger<StockLotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ConsumeAsync(int produitId, int reservoirId, decimal quantite, int periodeDetailId)
    {
        if (quantite <= 0)
        {
            _logger.LogWarning("ConsumeAsync called with zero or negative quantity: {Quantite}", quantite);
            return true; // Nothing to consume
        }

        _logger.LogInformation(
            "Consuming {Quantite}L from Reservoir {ReservoirId} for PeriodeDetail {PeriodeDetailId}",
            quantite, reservoirId, periodeDetailId);

        // Get available stock lots ordered by DateEntree (FIFO)
        var availableLots = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId 
                     && s.ProduitID == produitId
                     && s.Statut == "Disponible"
                     && s.QuantiteDisponible > 0)
            .OrderBy(s => s.DateEntree)
            .ToListAsync();

        if (!availableLots.Any())
        {
            _logger.LogError(
                "No available stock lots for Reservoir {ReservoirId}, Produit {ProduitId}",
                reservoirId, produitId);
            throw new InvalidOperationException(
                $"Stock insuffisant: Aucun lot disponible pour le réservoir {reservoirId}");
        }

        var totalAvailable = availableLots.Sum(l => l.QuantiteDisponible);
        if (totalAvailable < quantite)
        {
            _logger.LogError(
                "Insufficient stock: Required {Required}L, Available {Available}L for Reservoir {ReservoirId}",
                quantite, totalAvailable, reservoirId);
            throw new InvalidOperationException(
                $"Stock insuffisant: Requis {quantite:F3}L, Disponible {totalAvailable:F3}L");
        }

        var remainingToConsume = quantite;
        var consumptionDate = DateTime.UtcNow;

        foreach (var lot in availableLots)
        {
            if (remainingToConsume <= 0)
                break;

            var toConsumeFromLot = Math.Min(lot.QuantiteDisponible, remainingToConsume);

            // Create consumption record (audit trail)
            var consumption = new StockLotConsumption
            {
                StockLotID = lot.ID,
                PeriodeDetailID = periodeDetailId,
                QuantiteConsommee = toConsumeFromLot,
                PrixUnitaire = lot.PrixAchat,
                DateConsommation = consumptionDate
            };
            _context.StockLotConsumptions.Add(consumption);

            // Update lot
            lot.QuantiteDisponible -= toConsumeFromLot;
            if (lot.QuantiteDisponible <= 0)
            {
                lot.Statut = "Épuisé";
                _logger.LogInformation("StockLot {LotId} exhausted", lot.ID);
            }

            remainingToConsume -= toConsumeFromLot;

            _logger.LogDebug(
                "Consumed {Consumed}L from StockLot {LotId}, Remaining in lot: {Remaining}L",
                toConsumeFromLot, lot.ID, lot.QuantiteDisponible);
        }

        // Update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant -= quantite;
            if (reservoir.NiveauDeCarburant < 0)
            {
                _logger.LogWarning(
                    "Reservoir {ReservoirId} level went negative: {Level}L",
                    reservoirId, reservoir.NiveauDeCarburant);
            }
        }

        _logger.LogInformation(
            "Successfully consumed {Quantite}L from Reservoir {ReservoirId}",
            quantite, reservoirId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReverseConsumptionAsync(int periodeDetailId)
    {
        _logger.LogInformation("Reversing consumption for PeriodeDetail {PeriodeDetailId}", periodeDetailId);

        // Get all consumptions for this PeriodeDetail
        var consumptions = await _context.StockLotConsumptions
            .Include(c => c.StockLot)
            .Where(c => c.PeriodeDetailID == periodeDetailId)
            .ToListAsync();

        if (!consumptions.Any())
        {
            _logger.LogWarning("No consumptions found for PeriodeDetail {PeriodeDetailId}", periodeDetailId);
            return true; // Nothing to reverse
        }

        decimal totalRestored = 0;
        int? reservoirId = null;

        foreach (var consumption in consumptions)
        {
            var stockLot = consumption.StockLot;
            if (stockLot == null)
            {
                _logger.LogWarning("StockLot not found for consumption {ConsumptionId}", consumption.ID);
                continue;
            }

            // Restore quantity to the stock lot
            stockLot.QuantiteDisponible += consumption.QuantiteConsommee;
            
            // If lot was exhausted, make it available again
            if (stockLot.Statut == "Épuisé" && stockLot.QuantiteDisponible > 0)
            {
                stockLot.Statut = "Disponible";
            }

            totalRestored += consumption.QuantiteConsommee;
            reservoirId = stockLot.ReservoirID;

            _logger.LogDebug(
                "Restored {Quantite}L to StockLot {LotId}, New available: {Available}L",
                consumption.QuantiteConsommee, stockLot.ID, stockLot.QuantiteDisponible);
        }

        // Update reservoir level
        if (reservoirId.HasValue && totalRestored > 0)
        {
            var reservoir = await _context.Reservoirs.FindAsync(reservoirId.Value);
            if (reservoir != null)
            {
                reservoir.NiveauDeCarburant += totalRestored;
                _logger.LogInformation(
                    "Restored {Quantite}L to Reservoir {ReservoirId}, New level: {Level}L",
                    totalRestored, reservoirId.Value, reservoir.NiveauDeCarburant);
            }
        }

        // Delete the consumption records
        _context.StockLotConsumptions.RemoveRange(consumptions);

        _logger.LogInformation(
            "Successfully reversed {Quantite}L for PeriodeDetail {PeriodeDetailId}",
            totalRestored, periodeDetailId);

        return true;
    }

    /// <inheritdoc />
    public async Task CreateStockLotAsync(
        int achatId, 
        int reservoirId, 
        int produitId, 
        decimal quantite, 
        decimal prixAchat)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantite, nameof(quantite));

        var stockLot = new StockLot
        {
            AchatID = achatId,
            ReservoirID = reservoirId,
            ProduitID = produitId,
            QuantiteInitiale = quantite,
            QuantiteDisponible = quantite,
            PrixAchat = prixAchat,
            DateEntree = DateTime.UtcNow,
            Statut = "Disponible"
        };

        _context.StockLots.Add(stockLot);

        // Also update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant += quantite;
            _logger.LogInformation(
                "Updated Reservoir {ReservoirId} level to {Level}L after adding StockLot",
                reservoirId, reservoir.NiveauDeCarburant);
        }

        _logger.LogInformation(
            "Created StockLot: Achat {AchatId}, Reservoir {ReservoirId}, Quantite {Quantite}L, Prix {Prix}",
            achatId, reservoirId, quantite, prixAchat);
    }

    /// <inheritdoc />
    public async Task<decimal> GetAvailableStockAsync(int reservoirId)
    {
        return await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId && s.Statut == "Disponible")
            .SumAsync(s => s.QuantiteDisponible);
    }

    /// <inheritdoc />
    public async Task<bool> ReverseStockLotAsync(int achatId, int reservoirId, decimal quantite)
    {
        _logger.LogInformation(
            "Attempting to reverse StockLot: Achat {AchatId}, Reservoir {ReservoirId}, Qty {Quantite}L",
            achatId, reservoirId, quantite);

        // Find the StockLot created from this allocation
        var stockLot = await _context.StockLots
            .Include(s => s.Consumptions)
            .Where(s => s.AchatID == achatId && s.ReservoirID == reservoirId)
            .OrderByDescending(s => s.DateEntree) // Get the most recent one
            .FirstOrDefaultAsync();

        if (stockLot == null)
        {
            _logger.LogWarning(
                "No StockLot found to reverse for Achat {AchatId}, Reservoir {ReservoirId}",
                achatId, reservoirId);
            return false;
        }

        // Check if any consumption has occurred
        if (stockLot.Consumptions.Any())
        {
            _logger.LogWarning(
                "Cannot reverse StockLot {StockLotId} - it has {Count} consumption records",
                stockLot.ID, stockLot.Consumptions.Count);
            return false;
        }

        // Check if the quantity matches (or at least available)
        if (stockLot.QuantiteDisponible < quantite)
        {
            _logger.LogWarning(
                "Cannot reverse StockLot {StockLotId} - available {Available}L < requested {Requested}L",
                stockLot.ID, stockLot.QuantiteDisponible, quantite);
            return false;
        }

        // Mark as cancelled/remove
        stockLot.QuantiteDisponible = 0;
        stockLot.Statut = "Annulé";

        // Update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant = Math.Max(0, reservoir.NiveauDeCarburant - quantite);
            _logger.LogInformation(
                "Reversed Reservoir {ReservoirId} level to {Level}L",
                reservoirId, reservoir.NiveauDeCarburant);
        }

        _logger.LogInformation("Successfully reversed StockLot {StockLotId}", stockLot.ID);
        return true;
    }

    // ???????????????????????????????????????????????????????????????????????
    // SYNC OPERATIONS (Option C - Always Recalculate from Source of Truth)
    // ???????????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<decimal> SyncReservoirLevelAsync(int reservoirId)
    {
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
        {
            _logger.LogWarning("Reservoir {ReservoirId} not found for sync", reservoirId);
            return 0;
        }

        // Calculate level from StockLots (source of truth)
        var calculatedLevel = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId && s.Statut == "Disponible")
            .SumAsync(s => s.QuantiteDisponible);

        var oldLevel = reservoir.NiveauDeCarburant;
        reservoir.NiveauDeCarburant = calculatedLevel;

        if (Math.Abs(oldLevel - calculatedLevel) > 0.001m)
        {
            _logger.LogInformation(
                "Synced Reservoir {ReservoirId} level: {OldLevel}L ? {NewLevel}L (diff: {Diff}L)",
                reservoirId, oldLevel, calculatedLevel, calculatedLevel - oldLevel);
        }

        return calculatedLevel;
    }

    /// <inheritdoc />
    public async Task<bool> SyncPompeCountersAsync(int pompeId)
    {
        var pompe = await _context.Pompes.FindAsync(pompeId);
        if (pompe == null)
        {
            _logger.LogWarning("Pompe {PompeId} not found for sync", pompeId);
            return false;
        }

        // Find the most recent PeriodeDetail for this pump (by period end date)
        var latestDetail = await _context.PeriodeDetails
            .Include(d => d.Periode)
            .Where(d => d.PompeID == pompeId && d.Periode != null)
            .OrderByDescending(d => d.Periode!.DateFin)
            .FirstOrDefaultAsync();

        if (latestDetail == null)
        {
            _logger.LogDebug("No period details found for Pompe {PompeId}, counters unchanged", pompeId);
            return false;
        }

        var oldElec = pompe.CompteurElectroniqueActuel;
        var oldMeca = pompe.CompteurMecaniqueActuel;

        pompe.CompteurElectroniqueActuel = latestDetail.CompteurElectroniqueFinal;
        pompe.CompteurMecaniqueActuel = latestDetail.CompteurMecaniqueFinal;

        if (oldElec != latestDetail.CompteurElectroniqueFinal || 
            oldMeca != latestDetail.CompteurMecaniqueFinal)
        {
            _logger.LogInformation(
                "Synced Pompe {PompeId} counters from Periode {PeriodeId} ({DateFin:d}): " +
                "Elec {OldElec} ? {NewElec}, Meca {OldMeca} ? {NewMeca}",
                pompeId, latestDetail.PeriodeID, latestDetail.Periode?.DateFin,
                oldElec, pompe.CompteurElectroniqueActuel,
                oldMeca, pompe.CompteurMecaniqueActuel);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task SyncMultiplePompeCountersAsync(IEnumerable<int> pompeIds)
    {
        foreach (var pompeId in pompeIds.Distinct())
        {
            await SyncPompeCountersAsync(pompeId);
        }
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SyncMultipleReservoirLevelsAsync(IEnumerable<int> reservoirIds)
    {
        foreach (var reservoirId in reservoirIds.Distinct())
        {
            await SyncReservoirLevelAsync(reservoirId);
        }
        await _context.SaveChangesAsync();
    }

    // ???????????????????????????????????????????????????????????????????????
    // ANALYSIS OPERATIONS (Margin/Profit Reporting)
    // ???????????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<StockLotAnalysisDto?> GetStockLotAnalysisAsync(int stockLotId)
    {
        var stockLot = await _context.StockLots
            .Include(s => s.Reservoir)
            .Include(s => s.Produit)
            .Include(s => s.Consumptions)
                .ThenInclude(c => c.PeriodeDetail)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ID == stockLotId);

        if (stockLot == null)
            return null;

        return BuildStockLotAnalysis(stockLot);
    }

    /// <inheritdoc />
    public async Task<ReservoirAnalysisDto?> GetReservoirAnalysisAsync(int reservoirId)
    {
        var reservoir = await _context.Reservoirs
            .Include(r => r.Produit)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ID == reservoirId);

        if (reservoir == null)
            return null;

        var stockLots = await _context.StockLots
            .Include(s => s.Produit)
            .Include(s => s.Consumptions)
                .ThenInclude(c => c.PeriodeDetail)
            .Where(s => s.ReservoirID == reservoirId)
            .AsNoTracking()
            .ToListAsync();

        var lotAnalyses = stockLots.Select(BuildStockLotAnalysis).ToList();

        return new ReservoirAnalysisDto
        {
            ReservoirID = reservoir.ID,
            ReservoirNumero = reservoir.Numero,
            ProduitID = reservoir.ProduitID,
            ProduitNom = reservoir.Produit?.Description,
            CapaciteTotale = reservoir.Capacite,
            NiveauActuel = reservoir.NiveauDeCarburant,
            TotalStockLots = stockLots.Count,
            StockLotsDisponibles = stockLots.Count(s => s.Statut == "Disponible"),
            StockLotsEpuises = stockLots.Count(s => s.Statut == "Épuisé"),
            TotalQuantiteAchetee = lotAnalyses.Sum(l => l.QuantiteInitiale),
            TotalQuantiteVendue = lotAnalyses.Sum(l => l.QuantiteVendue),
            TotalQuantiteDisponible = lotAnalyses.Sum(l => l.QuantiteDisponible),
            TotalChiffreAffaires = lotAnalyses.Sum(l => l.ChiffreAffaires),
            TotalCoutRevient = lotAnalyses.Sum(l => l.CoutRevient),
            PrixAchatMoyen = lotAnalyses.Any() 
                ? Math.Round(lotAnalyses.Average(l => l.PrixAchat), 2) 
                : 0,
            PrixVenteMoyen = lotAnalyses.Where(l => l.QuantiteVendue > 0).Any()
                ? Math.Round(lotAnalyses.Where(l => l.QuantiteVendue > 0).Average(l => l.PrixVenteMoyen), 2)
                : 0,
            StockLots = lotAnalyses
        };
    }

    /// <inheritdoc />
    public async Task<GlobalAnalysisSummaryDto> GetGlobalAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        // Build consumption query with optional date filter
        var consumptionQuery = _context.StockLotConsumptions
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Produit)
            .Include(c => c.PeriodeDetail)
            .AsNoTracking();

        if (startDate.HasValue)
            consumptionQuery = consumptionQuery.Where(c => c.DateConsommation >= startDate.Value);
        if (endDate.HasValue)
            consumptionQuery = consumptionQuery.Where(c => c.DateConsommation <= endDate.Value);

        var consumptions = await consumptionQuery.ToListAsync();

        // Get all reservoirs with their stock lots
        var reservoirs = await _context.Reservoirs
            .Include(r => r.Produit)
            .Include(r => r.StockLots)
            .AsNoTracking()
            .ToListAsync();

        // Calculate per-product metrics
        var productGroups = consumptions
            .GroupBy(c => new { c.StockLot.ProduitID, c.StockLot.Produit?.Description })
            .Select(g => new ProductAnalysisSummaryDto
            {
                ProduitID = g.Key.ProduitID,
                ProduitNom = g.Key.Description,
                TotalQuantiteVendue = g.Sum(c => c.QuantiteConsommee),
                TotalCoutRevient = g.Sum(c => c.QuantiteConsommee * c.PrixUnitaire),
                TotalChiffreAffaires = g.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant)
            })
            .ToList();

        // Get reservoir analyses
        var reservoirAnalyses = new List<ReservoirAnalysisDto>();
        foreach (var reservoir in reservoirs.Where(r => r.StockLots.Any()))
        {
            var analysis = await GetReservoirAnalysisAsync(reservoir.ID);
            if (analysis != null)
            {
                // Clear nested lots for summary to reduce payload
                analysis.StockLots = [];
                reservoirAnalyses.Add(analysis);
            }
        }

        // Count distinct periodes
        var periodeIds = consumptions
            .Select(c => c.PeriodeDetail.PeriodeID)
            .Distinct()
            .Count();

        return new GlobalAnalysisSummaryDto
        {
            DateDebut = startDate,
            DateFin = endDate,
            TotalReservoirs = reservoirs.Count(r => r.StockLots.Any()),
            TotalStockLots = reservoirs.Sum(r => r.StockLots.Count),
            TotalPeriodesAnalysees = periodeIds,
            TotalQuantiteAchetee = reservoirs.Sum(r => r.StockLots.Sum(s => s.QuantiteInitiale)),
            TotalQuantiteVendue = consumptions.Sum(c => c.QuantiteConsommee),
            TotalQuantiteEnStock = reservoirs.Sum(r => r.StockLots.Where(s => s.Statut == "Disponible").Sum(s => s.QuantiteDisponible)),
            TotalChiffreAffaires = consumptions.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant),
            TotalCoutRevient = consumptions.Sum(c => c.QuantiteConsommee * c.PrixUnitaire),
            ParProduit = productGroups,
            ParReservoir = reservoirAnalyses
        };
    }

    /// <inheritdoc />
    public async Task<PeriodeMargeAnalysisDto?> GetPeriodeMargeAnalysisAsync(int periodeId)
    {
        _logger.LogInformation("Getting marge analysis for Periode {PeriodeId}", periodeId);

        // Get the periode
        var periode = await _context.Periodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PeriodeID == periodeId);

        if (periode == null)
        {
            _logger.LogWarning("Periode {PeriodeId} not found", periodeId);
            return null;
        }

        // Get all consumptions for this periode's details
        var consumptions = await _context.StockLotConsumptions
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Produit)
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Reservoir)
            .Include(c => c.PeriodeDetail)
                .ThenInclude(d => d.Pompe)
            .Where(c => c.PeriodeDetail.PeriodeID == periodeId)
            .AsNoTracking()
            .ToListAsync();

        // Build consumption details
        var consommationDetails = consumptions.Select(c => new ConsommationDetailDto
        {
            ConsumptionID = c.ID,
            StockLotID = c.StockLotID,
            PeriodeDetailID = c.PeriodeDetailID,
            ProduitID = c.StockLot.ProduitID,
            ProduitNom = c.StockLot.Produit?.Description,
            ReservoirID = c.StockLot.ReservoirID,
            ReservoirNumero = c.StockLot.Reservoir?.Numero,
            PompeID = c.PeriodeDetail.PompeID,
            PompeNumero = c.PeriodeDetail.Pompe?.Numero,
            QuantiteConsommee = c.QuantiteConsommee,
            PrixAchat = c.PrixUnitaire,
            PrixVente = c.PeriodeDetail.PrixCarburant,
            DateConsommation = c.DateConsommation
        }).ToList();

        // Aggregate by product
        var parProduit = consommationDetails
            .GroupBy(c => new { c.ProduitID, c.ProduitNom })
            .Select(g => new ProduitMargeDto
            {
                ProduitID = g.Key.ProduitID,
                ProduitNom = g.Key.ProduitNom,
                TotalQuantite = g.Sum(c => c.QuantiteConsommee),
                TotalCoutAchat = g.Sum(c => c.CoutAchat),
                TotalVente = g.Sum(c => c.Vente),
                PrixAchatMoyen = g.Sum(c => c.QuantiteConsommee) > 0
                    ? Math.Round(g.Sum(c => c.CoutAchat) / g.Sum(c => c.QuantiteConsommee), 2)
                    : 0,
                PrixVenteMoyen = g.Sum(c => c.QuantiteConsommee) > 0
                    ? Math.Round(g.Sum(c => c.Vente) / g.Sum(c => c.QuantiteConsommee), 2)
                    : 0
            })
            .ToList();

        var result = new PeriodeMargeAnalysisDto
        {
            PeriodeID = periodeId,
            DateDebut = periode.DateDebut,
            DateFin = periode.DateFin,
            Consommations = consommationDetails,
            ParProduit = parProduit,
            TotalQuantiteVendue = consommationDetails.Sum(c => c.QuantiteConsommee),
            TotalCoutAchat = consommationDetails.Sum(c => c.CoutAchat),
            TotalVente = consommationDetails.Sum(c => c.Vente)
        };

        _logger.LogInformation(
            "Periode {PeriodeId} marge analysis: {Count} consumptions, Marge={Marge:N2} ({Percent}%)",
            periodeId, consommationDetails.Count, result.TotalMarge, result.MargePercent);

        return result;
    }

    // ???????????????????????????????????????????????????????????????????????
    // PRIVATE HELPERS
    // ???????????????????????????????????????????????????????????????????????

    private static StockLotAnalysisDto BuildStockLotAnalysis(StockLot stockLot)
    {
        var quantiteVendue = stockLot.Consumptions.Sum(c => c.QuantiteConsommee);
        var chiffreAffaires = stockLot.Consumptions.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant);
        var coutRevient = stockLot.Consumptions.Sum(c => c.QuantiteConsommee * c.PrixUnitaire);

        var prixVenteMoyen = quantiteVendue > 0
            ? chiffreAffaires / quantiteVendue
            : 0;

        return new StockLotAnalysisDto
        {
            StockLotID = stockLot.ID,
            AchatID = stockLot.AchatID,
            ReservoirID = stockLot.ReservoirID,
            ProduitID = stockLot.ProduitID,
            ReservoirNumero = stockLot.Reservoir?.Numero,
            ProduitNom = stockLot.Produit?.Description,
            Statut = stockLot.Statut,
            DateEntree = stockLot.DateEntree,
            QuantiteInitiale = stockLot.QuantiteInitiale,
            QuantiteVendue = quantiteVendue,
            QuantiteDisponible = stockLot.QuantiteDisponible,
            PrixAchat = stockLot.PrixAchat,
            PrixVenteMoyen = Math.Round(prixVenteMoyen, 2),
            ChiffreAffaires = Math.Round(chiffreAffaires, 2),
            CoutRevient = Math.Round(coutRevient, 2)
        };
    }
}
