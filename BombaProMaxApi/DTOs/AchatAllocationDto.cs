namespace BombaProMaxApi.DTOs
{
    public class AchatAllocationDto
    {
        public int ID { get; set; }
        public int AchatID { get; set; }
        public int ReservoirID { get; set; }
        public decimal QuantiteAllouee { get; set; }
        public DateTime DateAllocation { get; set; }
        public string? Notes { get; set; }
        public string Statut { get; set; } = "En Attente";
        public string? UtilisateurAllocation { get; set; }

        // Display fields for Achat
        public string? AchatNumero { get; set; }

        // Display fields for Reservoir
        public string? ReservoirNumero { get; set; }
        public decimal? ReservoirCapacite { get; set; }
        public decimal? ReservoirNiveauActuel { get; set; }
        public decimal? ReservoirEspaceDisponible => ReservoirCapacite - ReservoirNiveauActuel;

        // Display fields for Product (fuel type)
        public int? ProduitID { get; set; }
        public string? ProduitNom { get; set; }
    }
}