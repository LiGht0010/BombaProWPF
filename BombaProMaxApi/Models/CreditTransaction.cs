using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class CreditTransaction
{
    [Key]
    public int CreditID { get; set; }

    [StringLength(20)]
    [Display(Name = "Numéro Transaction")]
    public string? NumeroTransaction { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientID { get; set; }

    [Display(Name = "Produit")]
    public int? ProduitID { get; set; }

    [Display(Name = "Service")]
    public int? ServiceID { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix TTC")]
    public decimal PrixTTC { get; set; }

    [Required]
    [Display(Name = "Quantité")]
    public int Quantite { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Montant Total")]
    public decimal MontantTotal { get; set; }

    [Required]
    [Display(Name = "Date Crédit")]
    public DateTime DateCredit { get; set; }

    [Required]
    [Display(Name = "Facturé")]
    public bool Facture { get; set; } = false; // false = Non Facturé, true = Facturé

    [Display(Name = "Facture Associée")]
    public int? FactureID { get; set; }

    // NEW: Link to BonLivraison (if converted to BL)
    [Display(Name = "Bon de Livraison")]
    public int? BonLivraisonID { get; set; }

    // NEW: Track if converted to BL
    [Display(Name = "En Bon de Livraison")]
    public bool EstEnBL { get; set; } = false;

    // Navigation properties
    [ForeignKey("ClientID")]
    [InverseProperty("CreditTransactions")]
    public virtual Client? Client { get; set; }

    [ForeignKey("ProduitID")]
    [InverseProperty("CreditTransactions")]
    public virtual Produit? Produit { get; set; }

    [ForeignKey("ServiceID")]
    [InverseProperty("CreditTransactions")]
    public virtual Service? Service { get; set; }

    [ForeignKey("FactureID")]
    [InverseProperty("CreditTransactions")]
    public virtual Facture? FactureAssociee { get; set; }

    // NEW: Navigation to BonLivraison
    [ForeignKey("BonLivraisonID")]
    public virtual BonLivraison? BonLivraison { get; set; }

    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
