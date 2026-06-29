namespace FourniPro.Models;

public class CreditFournisseurCardItem
{
    public int     CreditFournisseurId { get; set; }
    public string? NumeroCreditF       { get; set; }
    public DateOnly DateCredit         { get; set; }
    public string? AchatNumero         { get; set; }
    public string? FournisseurNom      { get; set; }
    public string? EmployeNom          { get; set; }
    public decimal? MontantTotal       { get; set; }
    public string? Statut              { get; set; }
    public string? ChequeReference     { get; set; }
    public string? StatutCheque        { get; set; }
}
