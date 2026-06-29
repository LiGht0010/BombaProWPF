using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class Fournisseur
{
    [Key]
    public int FournisseurId { get; set; }

    [StringLength(50)]
    public string? Prenom { get; set; }

    [StringLength(50)]
    public string? Nom { get; set; }

    [StringLength(50)]
    public string? Societe { get; set; }

    [StringLength(200)]
    public string? Adresse { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? RIB { get; set; }

    [StringLength(50)]
    public string? Contact { get; set; }

    [StringLength(50)]
    public string? ConditionsPaiement { get; set; }

    [StringLength(50)]
    public string Statut { get; set; } = "Actif";

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual ICollection<CreditFournisseur> CreditsFournisseur { get; set; } = [];

    // Audit
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
