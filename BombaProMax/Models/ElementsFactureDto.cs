namespace BombaProMax.Models
{
    public class ElementsFactureDto
    {
        public int ID { get; set; }
        public int? FactureID { get; set; }
        public int? ProduitID { get; set; }
        public int? ServiceID { get; set; }
        public decimal? Quantite { get; set; }
        
        /// <summary>
        /// Prix Unitaire (stored value, typically TTC for backwards compatibility)
        /// </summary>
        public decimal? PrixUnitaire { get; set; }
        
        /// <summary>
        /// Prix Unitaire Hors Taxe
        /// </summary>
        public decimal? PrixHT { get; set; }
        
        /// <summary>
        /// TVA percentage (e.g., 20 for 20%)
        /// </summary>
        public decimal? TVA { get; set; }
        
        /// <summary>
        /// Prix Toutes Taxes Comprises
        /// </summary>
        public decimal? PrixTTC { get; set; }

        // Display fields for related entities
        public string? FactureNumero { get; set; }
        public string? ProduitNom { get; set; }
        public string? ServiceNom { get; set; }
        
        // Calculated totals for the line
        public decimal MontantHT => (Quantite ?? 0) * (PrixHT ?? PrixUnitaire ?? 0);
        public decimal MontantTVA => MontantHT * ((TVA ?? 0) / 100);
        public decimal MontantTTC => MontantHT + MontantTVA;
    }
}
