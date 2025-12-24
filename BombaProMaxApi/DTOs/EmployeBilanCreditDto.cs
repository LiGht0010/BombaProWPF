namespace BombaProMaxApi.DTOs
{
    public class EmployeBilanCreditDto
    {
        public int BilanID { get; set; }
        public int EmployeID { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalPaye { get; set; }
        public decimal Balance { get; set; }

        // Display field for related entity
        public string? EmployeNom { get; set; }
    }
}
