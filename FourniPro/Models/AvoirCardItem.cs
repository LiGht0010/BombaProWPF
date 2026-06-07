namespace FourniPro.Models;

/// <summary>Compact row model for the Avoirs section table.</summary>
public class AvoirCardItem
{
    public int      AvoirId      { get; set; }
    public string?  NumeroAvoir  { get; set; }
    public DateOnly DateAvoir    { get; set; }

    // Source label — whichever is non-null
    public string?  NumeroVente  { get; set; }
    public string?  NumeroCredit { get; set; }

    public string?  ClientNom    { get; set; }
    public decimal? MontantAvoir { get; set; }
    public string?  Raison       { get; set; }
    public string?  EmployeNom   { get; set; }
    public string?  Note         { get; set; }

    /// <summary>Convenience: whichever source number is set.</summary>
    public string SourceNumero => NumeroVente ?? NumeroCredit ?? "—";

    /// <summary>"Vente" or "Crédit"</summary>
    public string SourceType => NumeroVente is not null ? "Vente" : "Crédit";
}
