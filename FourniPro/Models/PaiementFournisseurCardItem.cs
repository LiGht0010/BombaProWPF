namespace FourniPro.Models;

public class PaiementFournisseurCardItem
{
    public int      PaiementFournisseurId { get; set; }
    public int      CreditFournisseurId   { get; set; }
    public string?  NumeroCreditF         { get; set; }
    public DateOnly DatePaiement          { get; set; }
    public decimal? Montant               { get; set; }
    public string?  PaymentMethod         { get; set; }
    public string?  Reference             { get; set; }
    public string?  EmployeNom            { get; set; }
    public string?  Note                  { get; set; }
}
