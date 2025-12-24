namespace BombaProMax.Models
{
    public class DepenseDto
    {
        public int ID { get; set; }
        public string? Numero { get; set; }
        public DateOnly? Date { get; set; }
        public string? Categorie { get; set; }
        public decimal? Montant { get; set; }
        public string? Description { get; set; }
        public string? CreePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public string? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Helper property for display
        public string DateDisplay => Date?.ToString("dd/MM/yyyy") ?? "-";
        public string MontantDisplay => Montant?.ToString("N2") ?? "0.00";
    }
}
