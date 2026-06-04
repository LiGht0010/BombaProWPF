using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class Achat
{
    [Key]
    public int AchatId { get; set; }

    [StringLength(20)]
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

    // Audit
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
