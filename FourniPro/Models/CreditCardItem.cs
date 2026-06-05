namespace FourniPro.Models;

public class CreditCardItem
{
    public int CreditId { get; set; }
    public string? NumeroCredit { get; set; }
    public DateOnly DateCredit { get; set; }
    public string? ClientNom { get; set; }
    public string? ProduitNom { get; set; }
    public string? EmployeNom { get; set; }
    public int? VoyageID { get; set; }
    public string? VoyageNumero { get; set; }
    public int? Quantite { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? MontantTotal { get; set; }
    public string? Statut { get; set; }
    public string? ChequeReference { get; set; }
    public string? StatutCheque { get; set; }
}
