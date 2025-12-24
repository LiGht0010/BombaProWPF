using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Audit trail linking PeriodeDetail consumption to StockLot deductions.
/// Records exactly which stock lot was consumed, when, and how much.
/// Essential for FIFO cost tracking and audit compliance.
/// </summary>
public class StockLotConsumption
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// The stock lot that was consumed
    /// </summary>
    [Required]
    public int StockLotID { get; set; }

    /// <summary>
    /// The periode detail (sale) that consumed this stock
    /// </summary>
    [Required]
    public int PeriodeDetailID { get; set; }

    /// <summary>
    /// Quantity consumed from this specific lot (in liters)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(12, 3)")]
    [Display(Name = "Quantité Consommée")]
    public decimal QuantiteConsommee { get; set; }

    /// <summary>
    /// Unit price at time of consumption (captured from StockLot.PrixAchat for COGS)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix Unitaire")]
    public decimal PrixUnitaire { get; set; }

    /// <summary>
    /// Date and time when consumption occurred
    /// </summary>
    [Required]
    [Display(Name = "Date de Consommation")]
    public DateTime DateConsommation { get; set; }

    // Navigation properties
    [ForeignKey("StockLotID")]
    [InverseProperty("Consumptions")]
    public virtual StockLot StockLot { get; set; } = null!;

    [ForeignKey("PeriodeDetailID")]
    [InverseProperty("StockLotConsumptions")]
    public virtual PeriodeDetails PeriodeDetail { get; set; } = null!;

    /// <summary>
    /// Calculated cost of goods sold for this consumption
    /// </summary>
    [NotMapped]
    public decimal CoutTotal => QuantiteConsommee * PrixUnitaire;
}
