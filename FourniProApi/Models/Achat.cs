using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

/// <summary>Payment mode chosen at achat creation time.</summary>
public enum AchatModePaiement
{
    Immédiat,
    Crédit
}

/// <summary>Payment status of the achat when ModePaiement is Crédit.</summary>
public enum AchatStatut
{
    NonPayé,
    PartielPayé,
    Payé
}

public class Achat
{
    [Key]
    public int AchatId { get; set; }

    [StringLength(30)]
    public string? Numero { get; set; }

    public DateOnly Date { get; set; }

    public int? FournisseurID { get; set; }

    public int? ProduitID { get; set; }

    public int? Quantite { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cout { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrixAchatUnitaire { get; set; }

    public bool? LivraisonDefectueuse { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? VoyageID { get; set; }

    public int? EmployeId { get; set; }

    /// <summary>Stored as string — values from <see cref="AchatModePaiement"/>. Default: Immédiat.</summary>
    [StringLength(20)]
    public string? ModePaiement { get; set; } = "Immédiat";

    // ── Payment tracking (Crédit mode) ────────────────────────────────────────

    /// <summary>Amount already paid. Updated by payment automations.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontantPaye { get; set; } = 0;

    /// <summary>Stored as string — values from <see cref="AchatStatut"/>. Default: NonPayé.</summary>
    [StringLength(20)]
    public string? Statut { get; set; } = "NonPayé";

    /// <summary>Reference of the cheque left as guarantee. Nullable — null when no cheque.</summary>
    [StringLength(50)]
    public string? ChequeReference { get; set; }

    /// <summary>Nullable — null when no cheque. Values from <see cref="StatutCheque"/>.</summary>
    [StringLength(20)]
    public string? StatutCheque { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual Voyage? Voyage { get; set; }
    public virtual Employe? Employe { get; set; }
    public virtual ICollection<CreditFournisseur> CreditsFournisseur { get; set; } = [];

    // Audit
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
