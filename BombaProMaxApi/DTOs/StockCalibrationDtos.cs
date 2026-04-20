namespace BombaProMaxApi.DTOs;

/// <summary>
/// Request DTO for calibrating stock to match a jaugeage measurement.
/// </summary>
public class StockCalibrationRequestDto
{
    /// <summary>
    /// The Jaugeage ID to calibrate against
    /// </summary>
    public int JaugeageId { get; set; }

    /// <summary>
    /// User performing the calibration
    /// </summary>
    public string? UtilisateurCalibration { get; set; }

    /// <summary>
    /// Optional notes for the calibration
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Estimated unit price for adjustment lots (used when adding stock).
    /// If not provided, uses average price from existing lots.
    /// </summary>
    public decimal? PrixAchatEstime { get; set; }

    /// <summary>
    /// If true, only process specific reservoirs. If empty/null, process all in jaugeage.
    /// </summary>
    public List<int>? ReservoirIds { get; set; }
}

/// <summary>
/// Preview of calibration before applying changes.
/// </summary>
public class StockCalibrationPreviewDto
{
    public int JaugeageId { get; set; }
    public string? JaugeageNumero { get; set; }
    public DateTime DateJaugeage { get; set; }
    public string? TemoinNom { get; set; }

    /// <summary>
    /// Preview details per reservoir
    /// </summary>
    public List<ReservoirCalibrationPreviewDto> Reservoirs { get; set; } = [];

    /// <summary>
    /// Total adjustment needed across all reservoirs (positive = add, negative = reduce)
    /// </summary>
    public decimal TotalAdjustment => Reservoirs.Sum(r => r.Difference);

    /// <summary>
    /// Number of reservoirs that need adjustment
    /// </summary>
    public int ReservoirsNeedingAdjustment => Reservoirs.Count(r => Math.Abs(r.Difference) > 0.01m);
}

/// <summary>
/// Preview of calibration for a single reservoir.
/// </summary>
public class ReservoirCalibrationPreviewDto
{
    public int ReservoirId { get; set; }
    public string? ReservoirNumero { get; set; }
    public int ProduitId { get; set; }
    public string? ProduitNom { get; set; }

    /// <summary>
    /// Volume measured by jaugeage (from calibration table lookup)
    /// </summary>
    public decimal VolumeJaugeage { get; set; }

    /// <summary>
    /// Hauteur measured in cm
    /// </summary>
    public decimal HauteurMesuree { get; set; }

    /// <summary>
    /// Current system stock (sum of StockLots.QuantiteDisponible)
    /// </summary>
    public decimal StockSysteme { get; set; }

    /// <summary>
    /// Difference: VolumeJaugeage - StockSysteme
    /// Positive = need to add stock (create Adjustment lot)
    /// Negative = need to reduce stock (FIFO consume)
    /// </summary>
    public decimal Difference { get; set; }

    /// <summary>
    /// Percentage difference from system stock
    /// </summary>
    public decimal DifferencePercent => StockSysteme > 0 
        ? Math.Round((Difference / StockSysteme) * 100, 2) 
        : 0;

    /// <summary>
    /// Action that will be taken: "Ajouter", "Réduire", or "Aucun"
    /// </summary>
    public string Action => Difference > 0.01m ? "Ajouter" 
        : Difference < -0.01m ? "Réduire" 
        : "Aucun";

    /// <summary>
    /// Status indicator: "Excédent", "Déficit", or "OK"
    /// </summary>
    public string Statut => Difference > 0.01m ? "Excédent" 
        : Difference < -0.01m ? "Déficit" 
        : "OK";

    /// <summary>
    /// If reduction needed, can it be fully satisfied by available stock?
    /// </summary>
    public bool CanReduce { get; set; } = true;

    /// <summary>
    /// Warning message if reduction cannot be fully satisfied
    /// </summary>
    public string? WarningMessage { get; set; }
}

/// <summary>
/// Result of stock calibration operation.
/// </summary>
public class StockCalibrationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int JaugeageId { get; set; }
    public DateTime DateCalibration { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Results per reservoir
    /// </summary>
    public List<ReservoirCalibrationResultDto> Reservoirs { get; set; } = [];

    /// <summary>
    /// Total stock added across all reservoirs
    /// </summary>
    public decimal TotalStockAdded => Reservoirs.Sum(r => r.StockAdded);

    /// <summary>
    /// Total stock reduced across all reservoirs
    /// </summary>
    public decimal TotalStockReduced => Reservoirs.Sum(r => r.StockReduced);

    /// <summary>
    /// Number of adjustment StockLots created
    /// </summary>
    public int AdjustmentLotsCreated => Reservoirs.Count(r => r.AdjustmentLotId.HasValue);
}

/// <summary>
/// Result of calibration for a single reservoir.
/// </summary>
public class ReservoirCalibrationResultDto
{
    public int ReservoirId { get; set; }
    public string? ReservoirNumero { get; set; }
    public int ProduitId { get; set; }
    public string? ProduitNom { get; set; }

    /// <summary>
    /// Stock level before calibration
    /// </summary>
    public decimal StockBefore { get; set; }

    /// <summary>
    /// Stock level after calibration
    /// </summary>
    public decimal StockAfter { get; set; }

    /// <summary>
    /// Amount of stock added (if positive adjustment)
    /// </summary>
    public decimal StockAdded { get; set; }

    /// <summary>
    /// Amount of stock reduced (if negative adjustment)
    /// </summary>
    public decimal StockReduced { get; set; }

    /// <summary>
    /// ID of the Adjustment StockLot created (if any)
    /// </summary>
    public int? AdjustmentLotId { get; set; }

    /// <summary>
    /// Number of existing lots that were consumed/reduced
    /// </summary>
    public int LotsConsumed { get; set; }

    /// <summary>
    /// Action performed: "Ajout", "Réduction", or "Aucun"
    /// </summary>
    public string ActionPerformed { get; set; } = "Aucun";

    /// <summary>
    /// Any warning or error message
    /// </summary>
    public string? Message { get; set; }
}
