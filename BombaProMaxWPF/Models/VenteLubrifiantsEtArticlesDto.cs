namespace BombaProMaxWPF.Models
{
    public class VenteLubrifiantsEtArticlesDto
    {
        public int ID { get; set; }
        public string? NumeroVente { get; set; }
        public DateTime DateVente { get; set; }
        public int ProduitID { get; set; }
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaireTTC { get; set; }
        public int? ClientID { get; set; }
        public int? EmployeID { get; set; }
        public int? MoyenPaiementID { get; set; }
        public string? Notes { get; set; }
        public string? Statut { get; set; }
        public DateTime? DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public string? CreePar { get; set; }
        public string? ModifiePar { get; set; }

        // Calculated properties
        public decimal PrixUnitaireHT { get; set; }
        public decimal MontantTotalHT { get; set; }
        public decimal MontantTotalTTC { get; set; }
        public decimal MontantTVA { get; set; }
        public decimal? MargeBeneficiaire { get; set; }
        public decimal? TauxMarge { get; set; }
        public string CategorieNom { get; set; } = string.Empty;

        // Display fields for related entities
        public string? ProduitNom { get; set; }
        public string? ClientNom { get; set; }
        public string? EmployeNom { get; set; }
        public string? MoyenPaiementNom { get; set; }
    }
}
