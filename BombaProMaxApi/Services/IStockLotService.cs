using System.Threading.Tasks;
using BombaProMaxApi.DTOs;

namespace BombaProMaxApi.Services;

/// <summary>
/// Service for FIFO stock lot consumption during Periode creation.
/// This is NOT a CRUD service - StockLots are managed automatically.
/// </summary>
public interface IStockLotService
{
    // ???????????????????????????????????????????????????????????????????????
    // CONSUMPTION OPERATIONS
    // ???????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Consumes stock from a reservoir using FIFO order.
    /// Called after PeriodeDetails are created.
    /// </summary>
    /// <param name="produitId">Fuel product ID</param>
    /// <param name="reservoirId">Reservoir (tank) ID</param>
    /// <param name="quantite">Quantity to consume (liters)</param>
    /// <param name="periodeDetailId">The PeriodeDetail that caused this consumption</param>
    /// <returns>True if consumption succeeded, throws if insufficient stock</returns>
    Task<bool> ConsumeAsync(int produitId, int reservoirId, decimal quantite, int periodeDetailId);

    /// <summary>
    /// Reverses stock consumption for a specific PeriodeDetail.
    /// Called when editing or deleting a Periode.
    /// Restores stock to the original StockLots and updates Reservoir level.
    /// </summary>
    /// <param name="periodeDetailId">The PeriodeDetail whose consumption should be reversed</param>
    /// <returns>True if reversed successfully</returns>
    Task<bool> ReverseConsumptionAsync(int periodeDetailId);

    /// <summary>
    /// Creates a new StockLot when fuel is allocated to a reservoir from a purchase.
    /// Called from AchatAllocation flow.
    /// </summary>
    /// <param name="achatId">Source purchase ID</param>
    /// <param name="reservoirId">Target reservoir ID</param>
    /// <param name="produitId">Fuel product ID</param>
    /// <param name="quantite">Quantity allocated (liters)</param>
    /// <param name="prixAchat">Unit purchase price</param>
    Task CreateStockLotAsync(int achatId, int reservoirId, int produitId, decimal quantite, decimal prixAchat);

    /// <summary>
    /// Reverses a StockLot when an allocation is cancelled.
    /// Only works if the StockLot hasn't been consumed yet.
    /// </summary>
    /// <param name="achatId">The purchase ID</param>
    /// <param name="reservoirId">The reservoir ID</param>
    /// <param name="quantite">The quantity to reverse</param>
    /// <returns>True if reversed, false if already consumed</returns>
    Task<bool> ReverseStockLotAsync(int achatId, int reservoirId, decimal quantite);

    /// <summary>
    /// Gets total available stock for a reservoir.
    /// </summary>
    Task<decimal> GetAvailableStockAsync(int reservoirId);

    // ???????????????????????????????????????????????????????????????????????
    // ANALYSIS OPERATIONS (Margin/Profit Reporting)
    // ???????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Gets detailed analysis for a single StockLot.
    /// </summary>
    Task<StockLotAnalysisDto?> GetStockLotAnalysisAsync(int stockLotId);

    /// <summary>
    /// Gets aggregated analysis for all StockLots in a reservoir.
    /// </summary>
    Task<ReservoirAnalysisDto?> GetReservoirAnalysisAsync(int reservoirId);

    /// <summary>
    /// Gets global analysis summary across all reservoirs.
    /// </summary>
    Task<GlobalAnalysisSummaryDto> GetGlobalAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null);
}
