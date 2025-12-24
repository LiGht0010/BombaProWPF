using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

public class BonLivraisonDetails
{
    [Key]
    public int ID { get; set; }

    [Required]
    public int BonLivraisonID { get; set; }

    public int? ProduitID { get; set; }

    public int? ServiceID { get; set; }

    [Required]
    public int Quantite { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrixUnitaire { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontantLigne { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    // Navigation properties
    [ForeignKey("BonLivraisonID")]
    [InverseProperty("Details")]
    public virtual BonLivraison BonLivraison { get; set; } = null!;

    [ForeignKey("ProduitID")]
    [InverseProperty("BonLivraisonDetails")]
    public virtual Produit? Produit { get; set; }

    [ForeignKey("ServiceID")]
    [InverseProperty("BonLivraisonDetails")]
    public virtual Service? Service { get; set; }
}
