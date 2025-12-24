using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

[Table("ÉlémentsFacture")]
public partial class ElementsFacture
{
    [Key]
    public int ID { get; set; }
    public int? FactureID { get; set; }
    public int? ProduitID { get; set; }
    public int? ServiceID { get; set; }
    public int? Quantite { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PrixUnitaire { get; set; }
    [ForeignKey("FactureID")]
    [InverseProperty("ElementsFactures")]
    public virtual Facture? Facture { get; set; }
    [ForeignKey("ProduitID")]
    [InverseProperty("ElementsFactures")]
    public virtual Produit? Produit { get; set; }
    [ForeignKey("ServiceID")]
    [InverseProperty("ElementsFactures")]
    public virtual Service? Service { get; set; }
}
