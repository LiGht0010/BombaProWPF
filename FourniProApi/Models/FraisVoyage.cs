using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class FraisVoyage
{
    [Key]
    public int FraisVoyageId { get; set; }

    public int VoyageId { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual Voyage Voyage { get; set; } = null!;
}
