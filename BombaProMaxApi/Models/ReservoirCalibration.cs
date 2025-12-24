using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Stores the manufacturer's calibration table for a reservoir.
/// Maps height (cm) to volume (liters) for accurate gauging calculations.
/// </summary>
public class ReservoirCalibration
{
    [Key]
    public int ID { get; set; }

    [Required]
    public int ReservoirID { get; set; }

    /// <summary>
    /// Height measurement in centimeters (supports decimal values like 248.6)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(6, 2)")]
    [Display(Name = "Hauteur (cm)")]
    public decimal HauteurCm { get; set; }

    /// <summary>
    /// Corresponding volume in liters at this height
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Volume (L)")]
    public decimal VolumeLitres { get; set; }

    // Navigation property
    [ForeignKey("ReservoirID")]
    [InverseProperty("Calibrations")]
    public virtual Reservoir Reservoir { get; set; } = null!;
}
