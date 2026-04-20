namespace BombaProMaxWPF.Models
{
    public class CamionDto
    {
        public int ID { get; set; }
        public string? Matricule { get; set; }
        public string? Marque { get; set; }
        public int? CiterneID { get; set; }
        public int? FournisseurID { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display fields for related entities
        public string? FournisseurNom { get; set; }
        public string? CiterneNumero { get; set; }
    }
}
