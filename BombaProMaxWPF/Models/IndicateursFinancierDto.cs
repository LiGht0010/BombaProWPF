namespace BombaProMaxWPF.Models
{
    public class IndicateursFinancierDto
    {
        public int ID { get; set; }
        public DateOnly? DebutPeriode { get; set; }
        public DateOnly? FinPeriode { get; set; }
        public decimal? ChiffreAffairesTotal { get; set; }
        public decimal? DepensesTotales { get; set; }
        public decimal? BeneficeNet { get; set; }
        public decimal? TotalAchats { get; set; }
        public decimal? TotalVentes { get; set; }
        public decimal? TotalCarburantVendu { get; set; }
        public decimal? TotalProduitsVendus { get; set; }
        public decimal? TotalServicesVendus { get; set; }
        public DateTime? DateCreation { get; set; }
    }
}
