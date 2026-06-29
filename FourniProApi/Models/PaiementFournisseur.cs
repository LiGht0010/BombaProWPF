using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class PaiementFournisseur
{
    [Key]
    public int PaiementFournisseurId { get; set; }

    // ── FK → CreditFournisseur (required, cascade delete) ────────────────────

    public int CreditFournisseurId { get; set; }
    public virtual CreditFournisseur CreditFournisseur { get; set; } = null!;

    // ── Paiement ──────────────────────────────────────────────────────────────

    public DateOnly DatePaiement { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    /// <summary>Stored as string to stay consistent with the Credit/Vente pattern.</summary>
    [StringLength(20)]
    public string? PaymentMethod { get; set; }

    [StringLength(200)]
    public string? Reference { get; set; }

    [StringLength(500)]
    public string? ReferenceFile { get; set; }

    public string? Note { get; set; }

    // ── FK → Employe (optional, set-null) ────────────────────────────────────

    public int? EmployeId { get; set; }
    public virtual Employe? Employe { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
