namespace BombaProMaxWPF.Models
{
    public class ServiceDto
    {
        public int ID { get; set; }
        public string? Numero { get; set; }
        public string? Description { get; set; }
        public decimal? Prix { get; set; }
        
        // Category fields
        public int? ServiceCategorieID { get; set; }
        public string? ServiceCategorieNom { get; set; }
        
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
