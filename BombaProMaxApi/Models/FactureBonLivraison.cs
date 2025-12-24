using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

public class FactureBonLivraison
{
    [Key]
    public int ID { get; set; }

    [Required]
    public int FactureID { get; set; }

    [Required]
    public int BonLivraisonID { get; set; }

    public DateTime DateAssociation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("FactureID")]
    [InverseProperty("FactureBonLivraisons")]
    public virtual Facture Facture { get; set; } = null!;

    [ForeignKey("BonLivraisonID")]
    [InverseProperty("FactureBonLivraisons")]
    public virtual BonLivraison BonLivraison { get; set; } = null!;
}
