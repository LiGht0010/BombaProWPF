using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class Voyage
{
    [Key]
    public int VoyageId { get; set; }

    public int? CamionId { get; set; }
    public int? ChauffeurId { get; set; }
    public int? CiterneId { get; set; }

    public DateTime? DateDepart { get; set; }
    public DateTime? DateFinal { get; set; }

    [StringLength(200)]
    public string? LieuDepart { get; set; }

    [StringLength(200)]
    public string? LieuTerminal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? KilometrageDepart { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? KilometrageFinal { get; set; }

    [StringLength(20)]
    public string Statut { get; set; } = "InProgress";

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual Camion? Camion { get; set; }
    public virtual Chauffeur? Chauffeur { get; set; }
    public virtual Citerne? Citerne { get; set; }
    public virtual ICollection<StockVoyage> Stocks { get; set; } = [];
    public virtual ICollection<FraisVoyage> Frais { get; set; } = [];

    // ── Audit ─────────────────────────────────────────────────────────────────

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
