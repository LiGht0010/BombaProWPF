using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

/// <summary>
/// Represents the allocation of fuel purchases to storage tanks
/// </summary>
public partial class AchatAllocation
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// Foreign key to the purchase (Achat)
    /// </summary>
    public int AchatID { get; set; }

    /// <summary>
    /// Foreign key to the storage tank (Réservoir)
    /// </summary>
    public int ReservoirID { get; set; }

    /// <summary>
    /// Quantity allocated to this tank (in liters)
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal QuantiteAllouee { get; set; }

    /// <summary>
    /// Date and time when the allocation was made
    /// </summary>
    public DateTime DateAllocation { get; set; }

    /// <summary>
    /// Optional notes about the allocation
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Status of the allocation (En Attente, Confirmée, Annulée)
    /// </summary>
    [StringLength(50)]
    public string Statut { get; set; } = "En Attente";

    /// <summary>
    /// User who made the allocation (optional)
    /// </summary>
    [StringLength(100)]
    public string? UtilisateurAllocation { get; set; }

    // Navigation properties
    [ForeignKey("AchatID")]
    [InverseProperty("AchatAllocations")]
    public virtual Achat Achat { get; set; } = null!;

    [ForeignKey("ReservoirID")]
    [InverseProperty("AchatAllocations")]
    public virtual Reservoir Reservoir { get; set; } = null!;
}
