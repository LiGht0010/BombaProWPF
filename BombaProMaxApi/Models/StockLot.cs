using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Represents a purchase-based stock layer for FIFO inventory tracking.
/// Created when fuel is allocated to a reservoir from a purchase (Achat).
/// Consumed during Periode creation based on QuantiteVendue.
/// </summary>
public class StockLot
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// Source purchase that created this stock lot
    /// </summary>
    [Required]
    public int AchatID { get; set; }

    /// <summary>
    /// Storage tank where this stock resides
    /// </summary>
    [Required]
    public int ReservoirID { get; set; }

    /// <summary>
    /// Fuel type (product) in this lot
    /// </summary>
    [Required]
    public int ProduitID { get; set; }

    /// <summary>
    /// Original quantity when lot was created (immutable for audit)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(12, 3)")]
    [Display(Name = "Quantité Initiale")]
    public decimal QuantiteInitiale { get; set; }

    /// <summary>
    /// Current available quantity (decreases as consumed)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(12, 3)")]
    [Display(Name = "Quantité Disponible")]
    public decimal QuantiteDisponible { get; set; }

    /// <summary>
    /// Unit purchase price at time of purchase (for COGS/FIFO costing)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix d'Achat Unitaire")]
    public decimal PrixAchat { get; set; }

    /// <summary>
    /// Date when stock entered the reservoir (for FIFO ordering)
    /// </summary>
    [Required]
    [Display(Name = "Date d'Entrée")]
    public DateTime DateEntree { get; set; }

    /// <summary>
    /// Status of the lot: "Disponible", "Épuisé"
    /// </summary>
    [Required]
    [StringLength(20)]
    [Display(Name = "Statut")]
    public string Statut { get; set; } = "Disponible";

    // Navigation properties
    [ForeignKey("AchatID")]
    public virtual Achat Achat { get; set; } = null!;

    [ForeignKey("ReservoirID")]
    [InverseProperty("StockLots")]
    public virtual Reservoir Reservoir { get; set; } = null!;

    [ForeignKey("ProduitID")]
    public virtual Produit Produit { get; set; } = null!;

    /// <summary>
    /// Consumption records tracking how this lot was depleted
    /// </summary>
    [InverseProperty("StockLot")]
    public virtual ICollection<StockLotConsumption> Consumptions { get; set; } = new List<StockLotConsumption>();
}
