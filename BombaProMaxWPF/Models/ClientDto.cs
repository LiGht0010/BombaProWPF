namespace BombaProMaxWPF.Models
{
    public class ClientDto
    {
        public int ID { get; set; }
        public string NumeroClient { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string? Contact { get; set; }
        public string CIN { get; set; } = null!;
        public string NomSociete { get; set; } = null!;
        public int userID { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
