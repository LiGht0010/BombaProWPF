using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BombaProMaxApi.Data;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Services;

/// <summary>
/// Service for handling cascade updates when period details are modified.
/// When a PeriodeDetail's final counters change, subsequent periods' starting counters must be updated.
/// 
/// LINKING CONCEPT: DateDebut of Periode N+1 == DateFin of Periode N
/// This is how periods are chained together.
/// 
/// APPROACH: Hybrid (Option C)
/// - For EDITED period: Full reversal + reconsuption (handled by controller)
/// - For CASCADE periods: Delta adjustment only (preserves audit trail)
/// - For RESERVOIR: Sync from StockLots after all operations
/// - For POMPE: Sync from latest period after all operations
/// </summary>
public interface IPeriodeCascadeService
{
    /// <summary>
    /// Cascades counter updates to subsequent periods when a period's final counters are modified.
    /// Uses DELTA adjustment - only updates counters and adjusts stock difference, 
    /// does NOT reverse/recreate consumption records for cascaded periods.
    /// </summary>
    /// <param name="modifiedPeriodeId">The ID of the modified period</param>
    /// <param name="modifiedDetails">The modified detail entities with new final counter values</param>
    /// <returns>Cascade result with affected counts and IDs for syncing</returns>
    Task<CascadeResult> CascadeCounterUpdatesAsync(int modifiedPeriodeId, List<PeriodeDetails> modifiedDetails);

    /// <summary>
    /// Gets the next PeriodeDetail for a specific pump after a given period.
    /// Uses the linking concept: next period's DateDebut == this period's DateFin
    /// </summary>
    Task<PeriodeDetails?> GetNextPeriodeDetailForPompeAsync(int pompeId, int excludePeriodeId, DateTime linkDate);

    /// <summary>
    /// Gets all subsequent PeriodeDetails for a pump after a given period.
    /// Ordered by DateDebut for proper chain processing.
    /// </summary>
    Task<List<PeriodeDetails>> GetSubsequentPeriodeDetailsForPompeAsync(int pompeId, int excludePeriodeId, DateTime linkDate);
}

/// <summary>
/// Result of a cascade operation, containing info needed for syncing.
/// </summary>
public class CascadeResult
{
    public int AffectedPeriodeDetailCount { get; set; }
    public HashSet<int> AffectedPompeIds { get; set; } = [];
    public HashSet<int> AffectedReservoirIds { get; set; } = [];
    public decimal NetStockAdjustment { get; set; }
}

/// <summary>
/// Implementation of cascade update logic for period counter propagation.
/// </summary>
public class PeriodeCascadeService : IPeriodeCascadeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PeriodeCascadeService> _logger;

    public PeriodeCascadeService(
        AppDbContext context,
        ILogger<PeriodeCascadeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CascadeResult> CascadeCounterUpdatesAsync(int modifiedPeriodeId, List<PeriodeDetails> modifiedDetails)
    {
        var result = new CascadeResult();

        // Get the modified period's end date to find subsequent periods
        var modifiedPeriode = await _context.Periodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PeriodeID == modifiedPeriodeId);

        if (modifiedPeriode == null)
        {
            _logger.LogWarning("Modified periode {PeriodeId} not found for cascade", modifiedPeriodeId);
            return result;
        }

        _logger.LogInformation(
            "Starting cascade update for Periode {PeriodeId} (DateFin: {DateFin}), {DetailCount} details. " +
            "Looking for periods with DateDebut >= {DateFin}",
            modifiedPeriodeId, modifiedPeriode.DateFin, modifiedDetails.Count, modifiedPeriode.DateFin);

        // Track all affected pumps and reservoirs for later syncing
        foreach (var detail in modifiedDetails)
        {
            if (detail.PompeID.HasValue)
                result.AffectedPompeIds.Add(detail.PompeID.Value);
            if (detail.ReservoirID.HasValue)
                result.AffectedReservoirIds.Add(detail.ReservoirID.Value);
        }

        // For each modified detail, cascade to subsequent periods
        foreach (var modifiedDetail in modifiedDetails)
        {
            if (!modifiedDetail.PompeID.HasValue)
                continue;

            var pompeId = modifiedDetail.PompeID.Value;

            // Get ALL subsequent PeriodeDetails for this pump (chain cascade)
            // Exclude the modified period itself, find periods starting at or after DateFin
            var subsequentDetails = await GetSubsequentPeriodeDetailsForPompeAsync(
                pompeId, modifiedPeriodeId, modifiedPeriode.DateFin);

            if (!subsequentDetails.Any())
            {
                _logger.LogDebug(
                    "No subsequent periods found for Pompe {PompeId} after Periode {PeriodeId} (DateFin: {Date})",
                    pompeId, modifiedPeriodeId, modifiedPeriode.DateFin);
                continue;
            }

            _logger.LogInformation(
                "Found {Count} subsequent period(s) for Pompe {PompeId}: [{PeriodeIds}]",
                subsequentDetails.Count, pompeId,
                string.Join(", ", subsequentDetails.Select(d => d.PeriodeID)));

            // The first subsequent period should start where this one ended
            var expectedDebut = modifiedDetail.CompteurElectroniqueFinal;
            var expectedMecaDebut = modifiedDetail.CompteurMecaniqueFinal;

            foreach (var nextDetail in subsequentDetails)
            {
                // Track affected entities
                if (nextDetail.PompeID.HasValue)
                    result.AffectedPompeIds.Add(nextDetail.PompeID.Value);
                if (nextDetail.ReservoirID.HasValue)
                    result.AffectedReservoirIds.Add(nextDetail.ReservoirID.Value);

                // Check if update is needed
                if (nextDetail.CompteurElectroniqueDebut == expectedDebut &&
                    nextDetail.CompteurMecaniqueDebut == expectedMecaDebut)
                {
                    _logger.LogDebug(
                        "Cascade: Pompe {PompeId} in Periode {PeriodeId} already has correct counters, skipping",
                        pompeId, nextDetail.PeriodeID);
                    
                    // Counters match, but still update expected for next in chain
                    expectedDebut = nextDetail.CompteurElectroniqueFinal;
                    expectedMecaDebut = nextDetail.CompteurMecaniqueFinal;
                    continue;
                }

                // Calculate quantity change for delta adjustment
                var oldQuantite = nextDetail.QuantiteVendue;

                // Update starting counters
                var oldElecDebut = nextDetail.CompteurElectroniqueDebut;
                var oldMecaDebut = nextDetail.CompteurMecaniqueDebut;

                nextDetail.CompteurElectroniqueDebut = expectedDebut;
                nextDetail.CompteurMecaniqueDebut = expectedMecaDebut;

                // Recalculate quantity (computed from counters)
                var newQuantite = nextDetail.QuantiteVendue;
                var quantiteDelta = newQuantite - oldQuantite;

                _logger.LogInformation(
                    "Cascade: Pompe {PompeId} in Periode {PeriodeId} (DateDebut: {DateDebut}): " +
                    "ElecDebut {OldElec} ? {NewElec}, " +
                    "Quantite {OldQty:F2} ? {NewQty:F2} (delta: {Delta:F2}L)",
                    pompeId, nextDetail.PeriodeID, nextDetail.Periode?.DateDebut,
                    oldElecDebut, nextDetail.CompteurElectroniqueDebut,
                    oldQuantite, newQuantite, quantiteDelta);

                // Track net stock adjustment (will be applied via sync)
                result.NetStockAdjustment += quantiteDelta;
                result.AffectedPeriodeDetailCount++;

                // Set expected for next period in chain
                expectedDebut = nextDetail.CompteurElectroniqueFinal;
                expectedMecaDebut = nextDetail.CompteurMecaniqueFinal;
            }
        }

        // Save cascade changes (counter updates only, not stock)
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Cascade complete: {AffectedCount} period details updated, " +
            "net stock adjustment: {NetAdjust:F2}L, " +
            "affected pumps: {PumpCount}, affected reservoirs: {ResCount}",
            result.AffectedPeriodeDetailCount,
            result.NetStockAdjustment,
            result.AffectedPompeIds.Count,
            result.AffectedReservoirIds.Count);

        return result;
    }

    /// <inheritdoc />
    public async Task<PeriodeDetails?> GetNextPeriodeDetailForPompeAsync(int pompeId, int excludePeriodeId, DateTime linkDate)
    {
        // Find the next period for this pump
        // LINKING CONCEPT: Next period's DateDebut >= this period's DateFin
        // Exclude the current period to avoid self-reference
        return await _context.PeriodeDetails
            .Include(d => d.Periode)
            .Where(d => d.PompeID == pompeId &&
                        d.Periode != null &&
                        d.PeriodeID != excludePeriodeId &&  // Exclude the modified period
                        d.Periode.DateDebut >= linkDate)    // >= to catch exact match (DateDebut == DateFin)
            .OrderBy(d => d.Periode!.DateDebut)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<List<PeriodeDetails>> GetSubsequentPeriodeDetailsForPompeAsync(int pompeId, int excludePeriodeId, DateTime linkDate)
    {
        // Get all subsequent periods for this pump
        // LINKING CONCEPT: Next period's DateDebut >= this period's DateFin
        // Using >= because DateDebut of period N+1 typically EQUALS DateFin of period N
        return await _context.PeriodeDetails
            .Include(d => d.Periode)
            .Where(d => d.PompeID == pompeId &&
                        d.Periode != null &&
                        d.PeriodeID != excludePeriodeId &&  // Exclude the modified period
                        d.Periode.DateDebut >= linkDate)    // >= to catch exact match
            .OrderBy(d => d.Periode!.DateDebut)
            .ToListAsync();
    }
}
