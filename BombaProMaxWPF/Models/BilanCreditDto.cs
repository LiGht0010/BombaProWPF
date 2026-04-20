namespace BombaProMaxWPF.Models
{
    public class BilanCreditDto
    {
        public int BilanID { get; set; }
        public int ClientID { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalPaye { get; set; }
        public decimal Balance { get; set; }
        public decimal CreditFacture { get; set; }
        public decimal CreditNonFacture { get; set; }
        public DateTime DerniereMiseAJour { get; set; }
        public DateTime? PeriodeDebut { get; set; }
        public DateTime? PeriodeFin { get; set; }

        // Display field for related entity
        public string? ClientNom { get; set; }
    }
}
