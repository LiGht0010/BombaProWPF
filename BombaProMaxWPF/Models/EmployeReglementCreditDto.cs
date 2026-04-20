namespace BombaProMaxWPF.Models
{
    public class EmployeReglementCreditDto
    {
        public int ReglementID { get; set; }
        public int EmployeID { get; set; }
        public decimal MontantPaye { get; set; }
        public int ModePaiementID { get; set; }
        public string? ReferenceTransaction { get; set; }
        public string? ValidePar { get; set; }
        public DateTime DateReglement { get; set; }
        public string? Remarques { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display fields for related entities
        public string? EmployeNom { get; set; }
        public string? ModePaiementNom { get; set; }
    }
}
