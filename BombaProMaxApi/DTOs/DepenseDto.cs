namespace BombaProMaxApi.DTOs
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
    }
}
