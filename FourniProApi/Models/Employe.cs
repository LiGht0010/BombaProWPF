using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class Employe
{
    [Key]
    public int EmployeId { get; set; }

    [Required, StringLength(50)]
    public string Nom { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Prenom { get; set; } = string.Empty;

    [StringLength(20)]
    public string? CIN { get; set; }

    [StringLength(15)]
    public string? Telephone { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Poste { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Salaire { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual ICollection<Achat> Achats { get; set; } = [];
    public virtual ICollection<Vente> Ventes { get; set; } = [];
    public virtual ICollection<Credit> Credits { get; set; } = [];

    // ── Audit ─────────────────────────────────────────────────────────────────

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
