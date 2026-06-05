using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class StockVoyage
{
    [Key]
    public int StockVoyageId { get; set; }

    public int VoyageId { get; set; }
    public int? ProduitId { get; set; }

    public int? Quantite { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public virtual Voyage Voyage { get; set; } = null!;
    public virtual Produit? Produit { get; set; }
}
