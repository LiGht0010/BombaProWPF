namespace BombaProMax.Models
{
    public class CiterneDto
    {
        public int ID { get; set; }
        public string? MatriculeCiterne { get; set; }
        public decimal? Capacite { get; set; }
        public uint? PartitionsNumber { get; set; }
        public int? FournisseurID { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? FournisseurNom { get; set; }
    }
}
