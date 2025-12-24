namespace BombaProMax.Models
{
    public class ElementsFactureDto
    {
        public int ID { get; set; }
        public int? FactureID { get; set; }
        public int? ProduitID { get; set; }
        public int? ServiceID { get; set; }
        public int? Quantite { get; set; }
        public decimal? PrixUnitaire { get; set; }

        // Display fields for related entities
        public string? FactureNumero { get; set; }
        public string? ProduitNom { get; set; }
        public string? ServiceNom { get; set; }
    }
}
