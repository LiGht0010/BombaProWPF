using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Represents a stock layer for FIFO inventory tracking.
/// Created when fuel enters a reservoir (from purchase or opening balance).
/// Consumed during Periode creation based on QuantiteVendue.
/// </summary>
public class StockLot
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// The type/source of this stock lot.
    /// Determines validation rules for AchatID.
    /// </summary>
    [Required]
    public StockLotType Type { get; set; } = StockLotType.Purchase;

    /// <summary>
    /// Source purchase that created this stock lot.
    /// Required for Type = Purchase, must be null for OpeningBalance.
    /// </summary>
    public int? AchatID { get; set; }

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
    /// Unit purchase price at time of purchase (for COGS/FIFO costing).
    /// Can be 0 for OpeningBalance if cost is unknown.
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
    /// Status of the lot: "Disponible", "Épuisé", "Annulé"
    /// </summary>
    [Required]
    [StringLength(20)]
    [Display(Name = "Statut")]
    public string Statut { get; set; } = "Disponible";

    /// <summary>
    /// Optional notes for the stock lot (e.g., reason for opening balance)
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("AchatID")]
    public virtual Achat? Achat { get; set; }

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

    // ?????????????????????????????????????????????????????????????????
    // DOMAIN VALIDATION
    // ?????????????????????????????????????????????????????????????????

    /// <summary>
    /// Indicates if this is an opening balance lot (initial inventory).
    /// </summary>
    [NotMapped]
    public bool IsOpeningBalance => Type == StockLotType.OpeningBalance;

    /// <summary>
    /// Indicates if the cost is known (PrixAchat > 0).
    /// Opening balance lots may have unknown cost.
    /// </summary>
    [NotMapped]
    public bool HasKnownCost => PrixAchat > 0;

    /// <summary>
    /// Validates the StockLot according to domain rules.
    /// </summary>
    /// <returns>List of validation errors, empty if valid.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        // SL-2, SL-3: AchatID validation based on Type
        if (Type == StockLotType.Purchase && !AchatID.HasValue)
        {
            errors.Add("AchatID is required for Purchase type stock lots.");
        }
        if (Type != StockLotType.Purchase && AchatID.HasValue)
        {
            errors.Add($"AchatID must be null for {Type} type stock lots.");
        }

        // SL-4: QuantiteInitiale > 0
        if (QuantiteInitiale <= 0)
        {
            errors.Add("QuantiteInitiale must be greater than zero.");
        }

        // SL-5, SL-6: QuantiteDisponible bounds
        if (QuantiteDisponible < 0)
        {
            errors.Add("QuantiteDisponible cannot be negative.");
        }
        if (QuantiteDisponible > QuantiteInitiale)
        {
            errors.Add("QuantiteDisponible cannot exceed QuantiteInitiale.");
        }

        // SL-8: PrixAchat >= 0
        if (PrixAchat < 0)
        {
            errors.Add("PrixAchat cannot be negative.");
        }

        return errors;
    }
}
