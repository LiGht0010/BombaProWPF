namespace BombaProMaxWPF.Models
{
    public class AchatDto
    {
        public int ID { get; set; }
        public string? Numero { get; set; }
        public DateOnly Date { get; set; }
        public int? FournisseurID { get; set; }
        public int? ProduitID { get; set; }
        public int? Quantite { get; set; }
        public decimal? Cout { get; set; }
        public decimal? PrixAchatUnitaire { get; set; }
        public int? ChauffeurID { get; set; }
        public int? CamionID { get; set; }
        public bool? LivraisonDefectueuse { get; set; }

        // Audit fields
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display fields for related entities
        public string? FournisseurNom { get; set; }
        public string? ProduitNom { get; set; }
        public string? ChauffeurNom { get; set; }
        public string? CamionImmatriculation { get; set; }
    }
}