namespace FourniPro.Models;

public class CreditFournisseurDto
{
    public int CreditFournisseurId { get; set; }
    public string? NumeroCreditF { get; set; }
    public DateOnly DateCredit { get; set; }

    public int? AchatId { get; set; }
    public string? AchatNumero { get; set; }

    public int? FournisseurId { get; set; }
    public string? FournisseurNom { get; set; }

    public int? EmployeId { get; set; }
    public string? EmployeNom { get; set; }

    public decimal? MontantTotal { get; set; }

    public string? Statut { get; set; }

    /// <summary>Nullable — presence implies a cheque was left.</summary>
    public string? ChequeReference { get; set; }

    /// <summary>Nullable — null when no cheque.</summary>
    public string? StatutCheque { get; set; }

    public string? Note { get; set; }

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
