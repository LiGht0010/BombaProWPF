namespace BombaProMaxWPF.Models
{
    public class ProduitDto
    {
        public int ID { get; set; }
        public string NumeroProduit { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? PrixAchat { get; set; }
        public decimal? PrixHT { get; set; }
        public decimal? TVA { get; set; }
        public decimal? PrixTTC { get; set; }
        public decimal? MargeBeneficiaire { get; set; }
        public decimal? MargePourcentage { get; set; }
        public int? Stock { get; set; }
        public int? StockMinimum { get; set; }
        public int? DelaiDeLivraison { get; set; }
        public int? CategorieID { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? CategorieNom { get; set; }
    }
}
