namespace BombaProMaxWPF.Models;

public class ReservoirCalibrationDto
{
    public int ID { get; set; }
    public int ReservoirID { get; set; }
    public decimal HauteurCm { get; set; }
    public decimal VolumeLitres { get; set; }

    // Display field for related entity
    public string? ReservoirNumero { get; set; }
}

/// <summary>
/// DTO for bulk import of calibration data (CSV/Excel)
/// </summary>
public class ReservoirCalibrationImportDto
{
    public decimal HauteurCm { get; set; }
    public decimal VolumeLitres { get; set; }
}

/// <summary>
/// Response DTO for volume lookup by height
/// </summary>
public class VolumeLookupResultDto
{
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal HauteurCm { get; set; }
    public decimal VolumeLitres { get; set; }
    public bool IsInterpolated { get; set; }
}

/// <summary>
/// Response DTO for calibration import result
/// </summary>
public class CalibrationImportResultDto
{
    public string? Message { get; set; }
    public int Count { get; set; }
    public decimal HauteurMax { get; set; }
}
