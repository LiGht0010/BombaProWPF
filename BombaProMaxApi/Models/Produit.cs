using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Produit
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public string NumeroProduit { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    

    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix d'Achat")]
    public decimal? PrixAchat { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix HT (Hors Taxe)")]
    public decimal? PrixHT { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    [Display(Name = "TVA (%)")]
    public decimal? TVA { get; set; } = 20; // Default 20% TVA

    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix TTC (Prix de Vente)")]
    public decimal? PrixTTC { get; set; }

    // Calculated properties (not stored in database)
    [NotMapped]
    [Display(Name = "Marge Bénéficiaire")]
    public decimal? MargeBeneficiaire => PrixHT.HasValue && PrixAchat.HasValue
        ? PrixHT.Value - PrixAchat.Value
        : null;

    [NotMapped]
    [Display(Name = "Marge (%)")]
    public decimal? MargePourcentage => PrixAchat.HasValue && PrixAchat.Value > 0 && MargeBeneficiaire.HasValue
        ? Math.Round((MargeBeneficiaire.Value / PrixAchat.Value) * 100, 2)
        : null;

    public int? Stock { get; set; }

    public int? StockMinimum { get; set; }

    public int? DelaiDeLivraison { get; set; }


    // New foreign key for Categorie
    [Display(Name = "Catégorie")]
    public int? CategorieID { get; set; }
    [ForeignKey("CategorieID")]
    [InverseProperty("Produits")]
    public virtual Categorie? Categorie { get; set; }

    // Navigation property for Achats
    [InverseProperty("Produit")]
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();

    // Navigation property for CreditTransactions
    [InverseProperty("Produit")]
    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    // Navigation property for ElementsFactures
    [InverseProperty("Produit")]
    public virtual ICollection<ElementsFacture> ElementsFactures { get; set; } = new List<ElementsFacture>();

    // Navigation property for Reservoirs
    [InverseProperty("Produit")]
    public virtual ICollection<Reservoir> Reservoirs { get; set; } = new List<Reservoir>();

    // Navigation property for VentesLubrifiantsEtArticles
    [InverseProperty("Produit")]
    public virtual ICollection<VenteLubrifiantsEtArticles> VentesLubrifiantsEtArticles { get; set; } = new List<VenteLubrifiantsEtArticles>();

    // Navigation property for BonLivraisonDetails
    [InverseProperty("Produit")]
    public virtual ICollection<BonLivraisonDetails> BonLivraisonDetails { get; set; } = new List<BonLivraisonDetails>();

    // Helper method to calculate PrixTTC from PrixHT and TVA
    public void CalculatePrixTTC()
    {
        if (PrixHT.HasValue && TVA.HasValue)
        {
            PrixTTC = Math.Round(PrixHT.Value * (1 + TVA.Value / 100), 2);
        }
    }

    // Helper method to calculate PrixHT from PrixTTC and TVA
    public void CalculatePrixHT()
    {
        if (PrixTTC.HasValue && TVA.HasValue)
        {
            PrixHT = Math.Round(PrixTTC.Value / (1 + TVA.Value / 100), 2);
        }
    }






    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
