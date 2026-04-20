namespace BombaProMaxWPF.Models
{
    public class PompeDto
    {
        public int ID { get; set; }
        public string Numero { get; set; }
        public string Statut { get; set; }
        public decimal? CompteurElectroniqueActuel { get; set; }
        public decimal? CompteurMecaniqueActuel { get; set; }

        public int? ReservoirAssocieID { get; set; }

        // Display fields for reservoir
        public string? ReservoirNumero { get; set; }
        public decimal? ReservoirCapacite { get; set; }
        public decimal? ReservoirNiveauDeCarburant { get; set; }
        public string? CarburantNom { get; set; }

        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
