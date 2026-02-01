namespace BombaProMax.Models
{
    public class ChauffeurDto
    {
        public int ID { get; set; }
        public string Nom { get; set; } = null!;
        public string? Prenom { get; set; }
        public string? CIN { get; set; }
        public string? Telephone { get; set; }
        public string? NumeroPermis { get; set; }
        public int? FournisseurID { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? FournisseurNom { get; set; }

        /// <summary>
        /// Full name for display: "Prenom Nom" or just "Nom" if Prenom is empty
        /// </summary>
        public string FullName => string.IsNullOrWhiteSpace(Prenom) ? Nom : $"{Prenom} {Nom}";
    }
}
