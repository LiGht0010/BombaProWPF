namespace BombaProMaxWPF.Models
{
    public class PeriodeDetailsDto
    {
        public int PeriodeDetailID { get; set; }
        public int PeriodeID { get; set; }
        public int? PompeID { get; set; }
        public int? ReservoirID { get; set; }
        public int? ProduitID { get; set; }
        public decimal PrixCarburant { get; set; }

        // Dual meter system readings
        public decimal CompteurElectroniqueDebut { get; set; }
        public decimal CompteurElectroniqueFinal { get; set; }
        public decimal CompteurMecaniqueDebut { get; set; }
        public decimal CompteurMecaniqueFinal { get; set; }

        // Calculated properties
        public decimal QuantiteElectronique { get; set; }
        public decimal QuantiteMecanique { get; set; }
        public decimal DifferenceQuantite { get; set; }
        public decimal PrixTotalElectronique { get; set; }
        public decimal PrixTotalMecanique { get; set; }
        public decimal DifferenceValeur { get; set; }
        public decimal QuantiteVendue { get; set; }
        public decimal PrixTotal { get; set; }

        // Display fields for related entities
        public string? PompeNumero { get; set; }
        public string? ReservoirNumero { get; set; }
        public string? ProduitNom { get; set; }
    }
}
