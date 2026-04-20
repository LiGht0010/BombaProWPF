namespace BombaProMaxWPF.Models
{
    public class EmployeCreditTransactionDto
    {
        public int CreditID { get; set; }
        public string? NumeroTransaction { get; set; }
        public int EmployeID { get; set; }
        public decimal MontantTotal { get; set; }
        public DateTime DateCredit { get; set; }
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? EmployeNom { get; set; }
    }
}
