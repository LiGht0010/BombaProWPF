namespace BombaProMaxWPF.Models
{
    public class FournisseurDto
    {
        public int ID { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public string? Societe { get; set; }
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? RIB { get; set; }
        public string? Contact { get; set; }
        public string? ConditionsPaiement { get; set; }
        public string Statut { get; set; } = "Actif";

        // Audit fields
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
