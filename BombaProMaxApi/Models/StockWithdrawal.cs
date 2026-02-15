using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Audit record for manual stock withdrawals.
/// Tracks who withdrew what, when, and why.
/// </summary>
public class StockWithdrawal
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// The reservoir from which stock was withdrawn
    /// </summary>
    [Required]
    public int ReservoirID { get; set; }

    /// <summary>
    /// The product type that was withdrawn
    /// </summary>
    [Required]
    public int ProduitID { get; set; }

    /// <summary>
    /// Total quantity withdrawn in liters
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(12, 3)")]
    public decimal Quantite { get; set; }

    /// <summary>
    /// Reason/justification for the withdrawal
    /// </summary>
    [StringLength(500)]
    public string? Motif { get; set; }

    /// <summary>
    /// User who performed the withdrawal
    /// </summary>
    public int? UtilisateurID { get; set; }

    /// <summary>
    /// User name at time of withdrawal (denormalized for audit)
    /// </summary>
    [StringLength(100)]
    public string? UtilisateurNom { get; set; }

    /// <summary>
    /// Date and time of the withdrawal
    /// </summary>
    [Required]
    public DateTime DateRetrait { get; set; }

    /// <summary>
    /// Reservoir level before withdrawal
    /// </summary>
    [Column(TypeName = "decimal(12, 3)")]
    public decimal NiveauAvant { get; set; }

    /// <summary>
    /// Reservoir level after withdrawal
    /// </summary>
    [Column(TypeName = "decimal(12, 3)")]
    public decimal NiveauApres { get; set; }

    /// <summary>
    /// JSON serialized details of affected lots (for audit trail)
    /// </summary>
    [Column(TypeName = "text")]
    public string? LotsAffectesJson { get; set; }

    // Navigation properties
    [ForeignKey("ReservoirID")]
    public virtual Reservoir Reservoir { get; set; } = null!;

    [ForeignKey("ProduitID")]
    public virtual Produit Produit { get; set; } = null!;
}
