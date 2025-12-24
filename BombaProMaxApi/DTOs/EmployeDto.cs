namespace BombaProMaxApi.DTOs
{
    public class EmployeDto
    {
        public int ID { get; set; }
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public string? CIN { get; set; }
        public string? Telephone { get; set; }
        public string? Address { get; set; }
        public string? Poste { get; set; }
        public decimal? Salaire { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
