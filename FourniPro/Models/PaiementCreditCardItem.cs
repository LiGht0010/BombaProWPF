namespace FourniPro.Models;

public class PaiementCreditCardItem
{
    public int     PaiementCreditId { get; set; }
    public int     CreditId         { get; set; }
    public string? NumeroCredit     { get; set; }
    public DateOnly DatePaiement    { get; set; }
    public decimal? Montant         { get; set; }
    public string? PaymentMethod    { get; set; }
    public string? Reference        { get; set; }
    public string? EmployeNom       { get; set; }
    public string? Note             { get; set; }
}
