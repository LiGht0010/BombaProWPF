namespace FourniPro.Models;

public class PaiementCreditDto
{
    public int PaiementCreditId { get; set; }

    public int CreditId { get; set; }
    public string? NumeroCredit { get; set; }

    public DateOnly DatePaiement { get; set; }
    public decimal? Montant { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceFile { get; set; }
    public string? Note { get; set; }

    public int? EmployeId { get; set; }
    public string? EmployeNom { get; set; }

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
