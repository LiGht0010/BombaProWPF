namespace BombaProMaxApi.DTOs
{
    public class ServiceDto
    {
        public int ID { get; set; }
        public string? Numero { get; set; }
        public string? Description { get; set; }
        public decimal? Prix { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
