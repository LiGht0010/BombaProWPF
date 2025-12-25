using Microsoft.AspNetCore.Mvc;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Services;

namespace BombaProMaxApi.Controllers;

/// <summary>
/// Controller for StockLot analysis and margin reporting.
/// Read-only endpoints - StockLots are managed automatically by the system.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StockLotsController : ControllerBase
{
    private readonly IStockLotService _stockLotService;
    private readonly ILogger<StockLotsController> _logger;

    public StockLotsController(IStockLotService stockLotService, ILogger<StockLotsController> logger)
    {
        _stockLotService = stockLotService;
        _logger = logger;
    }

    // ???????????????????????????????????????????????????????????????????
    // ANALYSIS ENDPOINTS
    // ???????????????????????????????????????????????????????????????????

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
