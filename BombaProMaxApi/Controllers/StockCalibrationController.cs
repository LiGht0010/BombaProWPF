using BombaProMaxApi.DTOs;
using BombaProMaxApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BombaProMaxApi.Controllers;

/// <summary>
/// Controller for stock calibration operations based on jaugeage measurements.
/// Allows reconciling system stock with physical measurements.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StockCalibrationController : ControllerBase
{
    private readonly IStockLotService _stockLotService;
    private readonly ILogger<StockCalibrationController> _logger;

    public StockCalibrationController(
        IStockLotService stockLotService,
        ILogger<StockCalibrationController> logger)
    {
        _stockLotService = stockLotService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a preview of what calibration would do for a given jaugeage.
    /// Shows the difference between measured volumes and system stock per reservoir.
    /// </summary>
    /// <param name="jaugeageId">The jaugeage ID to preview calibration for</param>
    /// <returns>Preview with differences and proposed actions</returns>
    [HttpGet("preview/{jaugeageId}")]
    public async Task<ActionResult<StockCalibrationPreviewDto>> GetCalibrationPreview(int jaugeageId)
    {
        try
        {
            _logger.LogInformation("Getting calibration preview for Jaugeage {JaugeageId}", jaugeageId);
            
            var preview = await _stockLotService.GetCalibrationPreviewAsync(jaugeageId);
            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Calibration preview failed for Jaugeage {JaugeageId}", jaugeageId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calibration preview for Jaugeage {JaugeageId}", jaugeageId);
            return StatusCode(500, $"Erreur lors de la prévisualisation: {ex.Message}");
        }
    }

    /// <summary>
    /// Calibrates stock levels to match a jaugeage measurement.
    /// If measured volume > system: creates Adjustment StockLot.
    /// If measured volume < system: FIFO reduces existing lots.
    /// </summary>
    /// <param name="request">The calibration request</param>
    /// <returns>Result with details of adjustments made</returns>
    [HttpPost("calibrate")]
    public async Task<ActionResult<StockCalibrationResultDto>> CalibrateToJaugeage(
        [FromBody] StockCalibrationRequestDto request)
    {
        try
        {
            _logger.LogInformation(
                "Calibrating stock to Jaugeage {JaugeageId}, User: {User}",
                request.JaugeageId, request.UtilisateurCalibration);

            var result = await _stockLotService.CalibrateToJaugeageAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Calibration failed for Jaugeage {JaugeageId}", request.JaugeageId);
            return BadRequest(new StockCalibrationResultDto
            {
                Success = false,
                JaugeageId = request.JaugeageId,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calibrating to Jaugeage {JaugeageId}", request.JaugeageId);
            return StatusCode(500, new StockCalibrationResultDto
            {
                Success = false,
                JaugeageId = request.JaugeageId,
                Message = $"Erreur lors de la calibration: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates a manual adjustment StockLot (without jaugeage reference).
    /// Use this for manual stock corrections.
    /// </summary>
    /// <param name="request">The adjustment request</param>
    /// <returns>The created StockLot ID</returns>
    [HttpPost("adjustment")]
    public async Task<ActionResult<object>> CreateAdjustment([FromBody] CreateAdjustmentRequestDto request)
    {
        try
        {
            _logger.LogInformation(
                "Creating manual adjustment for Reservoir {ReservoirId}: {Quantite}L",
                request.ReservoirId, request.Quantite);

            var lotId = await _stockLotService.CreateAdjustmentAsync(
                request.ReservoirId,
                request.ProduitId,
                request.Quantite,
                request.PrixAchat,
                request.JaugeageId,
                request.Notes);

            return Ok(new 
            { 
                Success = true,
                StockLotId = lotId,
                Message = $"Lot d'ajustement créé avec succès: +{request.Quantite:N2}L"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Adjustment creation failed for Reservoir {ReservoirId}", request.ReservoirId);
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating adjustment for Reservoir {ReservoirId}", request.ReservoirId);
            return StatusCode(500, new { Success = false, Message = $"Erreur: {ex.Message}" });
        }
    }
}

/// <summary>
/// Request DTO for creating a manual adjustment StockLot.
/// </summary>
public class CreateAdjustmentRequestDto
{
    /// <summary>
    /// The reservoir ID to adjust
    /// </summary>
    public int ReservoirId { get; set; }

    /// <summary>
    /// The product ID
    /// </summary>
    public int ProduitId { get; set; }

    /// <summary>
    /// Quantity to add (positive only)
    /// </summary>
    public decimal Quantite { get; set; }

    /// <summary>
    /// Estimated unit price (can be 0)
    /// </summary>
    public decimal PrixAchat { get; set; }

    /// <summary>
    /// Optional jaugeage reference
    /// </summary>
    public int? JaugeageId { get; set; }

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}
