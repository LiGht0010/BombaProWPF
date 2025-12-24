using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models
{
    [Table("VenteLubrifiantsEtArticles")]
    public partial class VenteLubrifiantsEtArticles
    {
        private const decimal TVA_RATE = 0.2M;

        [Key]
        public int ID { get; set; }

        [StringLength(20)]
        [Display(Name = "Numéro de Vente")]
        public string? NumeroVente { get; set; }

        [Required(ErrorMessage = "La date de vente est obligatoire")]
        [Display(Name = "Date de Vente")]
        public DateTime DateVente { get; set; }

        [Required(ErrorMessage = "Le produit est obligatoire")]
        [Display(Name = "Produit")]
        public int ProduitID { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0")]
        [Display(Name = "Quantité Vendue")]
        public int QuantiteVendue { get; set; }

        [Required(ErrorMessage = "Le prix unitaire de vente est obligatoire")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "Le prix unitaire doit être entre 0.01 et 99,999,999.99")]
        [Display(Name = "Prix Unitaire TTC")]
        public decimal PrixUnitaireTTC { get; set; }

        [Display(Name = "Client")]
        public int? ClientID { get; set; }

        [Display(Name = "Employé Vendeur")]
        public int? EmployeID { get; set; }

        [Display(Name = "Mode de Paiement")]
        public int? MoyenPaiementID { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes/Observations")]
        public string? Notes { get; set; }

        [StringLength(50)]
        [Display(Name = "Statut de la Vente")]
        public string? Statut { get; set; } = "Confirmée";

        [NotMapped]
        [Display(Name = "Prix Unitaire HT")]
        public decimal PrixUnitaireHT => PrixUnitaireTTC / (1 + TVA_RATE);

        [NotMapped]
        [Display(Name = "Montant Total HT")]
        public decimal MontantTotalHT => PrixUnitaireHT * QuantiteVendue;

        [NotMapped]
        [Display(Name = "Montant Total TTC")]
        public decimal MontantTotalTTC => PrixUnitaireTTC * QuantiteVendue;

        [NotMapped]
        [Display(Name = "Montant TVA")]
        public decimal MontantTVA => MontantTotalTTC - MontantTotalHT;

        [NotMapped]
        [Display(Name = "Marge Bénéficiaire")]
        public decimal? MargeBeneficiaire
        {
            get
            {
                if (Produit?.PrixAchat.HasValue == true)
                {
                    var coutTotal = Produit.PrixAchat.Value * QuantiteVendue;
                    return MontantTotalHT - coutTotal;
                }
                return null;
            }
        }

        [NotMapped]
        [Display(Name = "Taux de Marge (%)")]
        public decimal? TauxMarge
        {
            get
            {
                if (Produit?.PrixAchat.HasValue == true && Produit.PrixAchat.Value > 0)
                {
                    var coutUnitaire = Produit.PrixAchat.Value;
                    var margeUnitaire = PrixUnitaireHT - coutUnitaire;
                    return (margeUnitaire / coutUnitaire) * 100;
                }
                return null;
            }
        }

        [NotMapped]
        public bool IsLubrifiant => Produit?.Categorie?.Nom?.ToLower() == "lubrifiant";

        [NotMapped]
        public bool IsArticle => Produit?.Categorie?.Nom?.ToLower() == "articles";

        [NotMapped]
        [Display(Name = "Catégorie")]
        public string CategorieNom => Produit?.Categorie?.Nom ?? "Non définie";

        [ForeignKey("ProduitID")]
        [InverseProperty("VentesLubrifiantsEtArticles")]
        public virtual Produit Produit { get; set; } = null!;

        [ForeignKey("ClientID")]
        public virtual Client? Client { get; set; }

        [ForeignKey("EmployeID")]
        public virtual Employe? Employe { get; set; }

        [ForeignKey("MoyenPaiementID")]
        public virtual MoyensPaiement? MoyenPaiement { get; set; }

        public bool IsValidProductCategory()
        {
            if (Produit?.Categorie?.Nom == null) return false;
            var categoryName = Produit.Categorie.Nom.ToLower();
            return categoryName == "lubrifiant" || categoryName == "articles";
        }

        public bool IsPriceConsistentWithProduct()
        {
            if (Produit?.PrixTTC.HasValue == true)
            {
                var variance = Math.Abs(PrixUnitaireTTC - Produit.PrixTTC.Value) / Produit.PrixTTC.Value;
                return variance <= 0.05m;
            }
            return true;
        }

        public string GenerateNumeroVente()
        {
            var prefix = IsLubrifiant ? "LUB" : "ART";
            var date = DateVente.ToString("yyyyMMdd");
            var time = DateVente.ToString("HHmmss");
            return $"{prefix}-{date}-{time}";
        }

        public bool UpdateProductStock()
        {
            if (Produit?.Stock.HasValue == true)
            {
                if (Produit.Stock.Value >= QuantiteVendue)
                {
                    Produit.Stock -= QuantiteVendue;
                    return true;
                }
                return false;
            }
            return true;
        }

        [Display(Name = "Date de Création")]
        public DateTime? DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Date de Modification")]
        public DateTime? DateModification { get; set; }

        [StringLength(50)]
        [Display(Name = "Créé Par")]
        public string? CreePar { get; set; }

        [StringLength(50)]
        [Display(Name = "Modifié Par")]
        public string? ModifiePar { get; set; }
    }

    public class ValidProductCategoryAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is VenteLubrifiantsEtArticles vente)
            {
                return vente.IsValidProductCategory();
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Le produit doit appartenir à la catégorie 'Lubrifiant' ou 'Articles'.";
        }
    }

    public class VenteLubrifiantsEtArticlesReportDto
    {
        public string CategorieNom { get; set; } = string.Empty;
        public string ProduitDescription { get; set; } = string.Empty;
        public int TotalQuantite { get; set; }
        public decimal TotalMontantHT { get; set; }
        public decimal TotalMontantTTC { get; set; }
        public decimal MoyennePrixUnitaire { get; set; }
        public int NombreVentes { get; set; }
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
    }

    public class VenteLubrifiantsEtArticlesViewModel
    {
        public VenteLubrifiantsEtArticles Vente { get; set; } = new();
        public List<Produit> ProduitsDisponibles { get; set; } = new();
        public List<Client> Clients { get; set; } = new();
        public List<Employe> Employes { get; set; } = new();
        public List<MoyensPaiement> MoyensPaiements { get; set; } = new();
        public int? StockDisponible { get; set; }
        public bool StockInsuffisant => StockDisponible.HasValue && StockDisponible < Vente.QuantiteVendue;
    }
}
