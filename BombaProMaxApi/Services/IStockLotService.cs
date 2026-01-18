using System.Threading.Tasks;
using BombaProMaxApi.DTOs;

namespace BombaProMaxApi.Services;

/// <summary>
/// Service for FIFO stock lot management and consumption during Periode creation.
/// Handles stock creation (from purchases and opening balances) and consumption.
/// </summary>
public interface IStockLotService
{
    // ?????????????????????????????????????????????????????????????????
    // STOCK CREATION OPERATIONS
    // ?????????????????????????????????????????????????????????????????

    /// <summary>
    /// Creates a new StockLot when fuel is allocated to a reservoir from a purchase.
    /// Sets Type = Purchase and requires AchatID.
    /// </summary>
    Task CreateStockLotAsync(int achatId, int reservoirId, int produitId, decimal quantite, decimal prixAchat);

    /// <summary>
    /// Creates an Opening Balance StockLot for initial inventory.
    /// Used during onboarding when a reservoir has existing fuel.
    /// Only one Opening Balance lot is allowed per reservoir.
    /// </summary>
    /// <param name="reservoirId">The reservoir ID</param>
    /// <param name="produitId">The product/fuel type ID</param>
    /// <param name="quantite">Initial quantity in liters</param>
    /// <param name="prixAchat">Estimated purchase price (can be 0 if unknown)</param>
    /// <param name="dateEntree">Date when stock entered (defaults to now)</param>
    /// <param name="notes">Optional notes explaining the opening balance</param>
    /// <returns>The created StockLot ID</returns>
    Task<int> CreateOpeningBalanceAsync(
        int reservoirId, 
        int produitId, 
        decimal quantite, 
        decimal prixAchat,
        DateTime? dateEntree = null,
        string? notes = null);

    /// <summary>
    /// Checks if a reservoir already has an Opening Balance StockLot.
    /// </summary>
    /// <param name="reservoirId">The reservoir ID to check</param>
    /// <returns>True if an opening balance exists</returns>
    Task<bool> HasOpeningBalanceAsync(int reservoirId);

    /// <summary>
    /// Checks if a reservoir has any StockLots (of any type).
    /// </summary>
    /// <param name="reservoirId">The reservoir ID to check</param>
    /// <returns>True if any stock lots exist</returns>
    Task<bool> HasAnyStockLotsAsync(int reservoirId);

    /// <summary>
    /// Validates that stock can be added to a reservoir (capacity check).
    /// </summary>
    /// <param name="reservoirId">The reservoir ID</param>
    /// <param name="quantiteToAdd">Quantity to add in liters</param>
    /// <returns>True if capacity allows, false if would overfill</returns>
    Task<bool> ValidateCapacityAsync(int reservoirId, decimal quantiteToAdd);

    // ?????????????????????????????????????????????????????????????????
    // CONSUMPTION OPERATIONS
    // ?????????????????????????????????????????????????????????????????

    /// <summary>
    /// Consumes stock from a reservoir using FIFO order.
    /// Called after PeriodeDetails are created.
    /// </summary>
    Task<bool> ConsumeAsync(int produitId, int reservoirId, decimal quantite, int periodeDetailId);

    /// <summary>
    /// Reverses stock consumption for a specific PeriodeDetail.
    /// Called when editing or deleting a Periode.
    /// </summary>
    Task<bool> ReverseConsumptionAsync(int periodeDetailId);

    /// <summary>
    /// Reverses a StockLot when an allocation is cancelled.
    /// </summary>
    Task<bool> ReverseStockLotAsync(int achatId, int reservoirId, decimal quantite);

    /// <summary>
    /// Gets total available stock for a reservoir.
    /// </summary>
    Task<decimal> GetAvailableStockAsync(int reservoirId);

    /// <summary>
    /// Validates that sufficient stock exists for a sale.
    /// </summary>
    /// <param name="reservoirId">The reservoir ID</param>
    /// <param name="produitId">The product ID</param>
    /// <param name="quantiteRequired">Required quantity in liters</param>
    /// <returns>True if stock is sufficient</returns>
    Task<bool> ValidateStockAvailabilityAsync(int reservoirId, int produitId, decimal quantiteRequired);

    // ?????????????????????????????????????????????????????????????????
    // SYNC OPERATIONS (Recalculate from Source of Truth)
    // ?????????????????????????????????????????????????????????????????

    /// <summary>
    /// Recalculates and syncs a Reservoir's NiveauDeCarburant from its StockLots.
    /// This is the "source of truth" approach - reservoir level = sum of available stock.
    /// Call this after any operation that might affect stock levels.
    /// </summary>
    /// <param name="reservoirId">The reservoir to sync</param>
    /// <returns>The new calculated level</returns>
    Task<decimal> SyncReservoirLevelAsync(int reservoirId);

    /// <summary>
    /// Syncs a Pompe's current counters from its most recent PeriodeDetail.
    /// Pompe.CompteurActuel = Latest Period's CompteurFinal.
    /// </summary>
    /// <param name="pompeId">The pump to sync</param>
    /// <returns>True if synced, false if no periods exist for this pump</returns>
    Task<bool> SyncPompeCountersAsync(int pompeId);

    /// <summary>
    /// Syncs all Pompes that were involved in a specific Periode.
    /// Call this after create/update/delete of a periode.
    /// </summary>
    /// <param name="pompeIds">List of pump IDs to sync</param>
    Task SyncMultiplePompeCountersAsync(IEnumerable<int> pompeIds);

    /// <summary>
    /// Syncs all Reservoirs that were involved in a specific Periode.
    /// Call this after create/update/delete of a periode.
    /// </summary>
    /// <param name="reservoirIds">List of reservoir IDs to sync</param>
    Task SyncMultipleReservoirLevelsAsync(IEnumerable<int> reservoirIds);

    // ?????????????????????????????????????????????????????????????????
    // ANALYSIS OPERATIONS (Margin/Profit Reporting)
    // ?????????????????????????????????????????????????????????????????

    Task<StockLotAnalysisDto?> GetStockLotAnalysisAsync(int stockLotId);
    Task<ReservoirAnalysisDto?> GetReservoirAnalysisAsync(int reservoirId);
    Task<GlobalAnalysisSummaryDto> GetGlobalAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Gets margin analysis for a specific Periode.
    /// Shows which StockLots were consumed at what PrixAchat and calculates margins.
    /// </summary>
    /// <param name="periodeId">The Periode ID to analyze</param>
    /// <returns>Detailed margin breakdown with FIFO cost tracking</returns>
    Task<PeriodeMargeAnalysisDto?> GetPeriodeMargeAnalysisAsync(int periodeId);
}
