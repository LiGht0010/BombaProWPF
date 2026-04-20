namespace BombaProMaxWPF.Models;

/// <summary>
/// Data Transfer Object for DepotCaisse (cash deposit to bank).
/// </summary>
public class DepotCaisseDto
{
    public int ID { get; set; }

    /// <summary>
    /// Amount deposited to the bank.
    /// </summary>
    public decimal Montant { get; set; }

    /// <summary>
    /// Date and time of the deposit.
    /// </summary>
    public DateTime DateDepot { get; set; }

    /// <summary>
    /// Bank reference or receipt number.
    /// </summary>
    public string? ReferenceBancaire { get; set; }

    /// <summary>
    /// Name of the bank or branch.
    /// </summary>
    public string? Banque { get; set; }

    /// <summary>
    /// Additional notes about the deposit.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Confirmation document file name.
    /// </summary>
    public string? PieceJustificativeNom { get; set; }

    /// <summary>
    /// Confirmation document as Base64 encoded string.
    /// </summary>
    public string? PieceJustificativeBase64 { get; set; }

    /// <summary>
    /// MIME type of the confirmation document.
    /// </summary>
    public string? PieceJustificativeType { get; set; }

    /// <summary>
    /// User ID who validated this deposit.
    /// </summary>
    public int? ValidePar { get; set; }

    /// <summary>
    /// Name of the user who validated (populated from API).
    /// </summary>
    public string? ValidateurNom { get; set; }

    // Audit fields
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Display helpers
    public string DateDisplay => DateDepot.ToString("dd/MM/yyyy HH:mm");
    public string MontantDisplay => $"{Montant:N2} MAD";
    public bool HasPieceJustificative => !string.IsNullOrEmpty(PieceJustificativeBase64);
}

/// <summary>
/// Summary of cash in the station from all sources.
/// </summary>
public class CaisseSummaryDto
{
    /// <summary>
    /// Total cash from Periodes (Espèces field).
    /// </summary>
    public decimal EspecesPeriodes { get; set; }

    /// <summary>
    /// Total cash from lubricant/article sales paid in cash.
    /// </summary>
    public decimal EspecesVenteLubArticles { get; set; }

    /// <summary>
    /// Total cash from service sales paid in cash.
    /// </summary>
    public decimal EspecesVenteServices { get; set; }

    /// <summary>
    /// Total cash from credit payments (ReglementCredit) paid in cash.
    /// </summary>
    public decimal EspecesReglementCredits { get; set; }

    /// <summary>
    /// Total cash deposited to bank.
    /// </summary>
    public decimal TotalDepots { get; set; }

    /// <summary>
    /// Current cash balance in station (Total received - Total deposited).
    /// </summary>
    public decimal SoldeActuel => EspecesPeriodes + EspecesVenteLubArticles + EspecesVenteServices + EspecesReglementCredits - TotalDepots;

    /// <summary>
    /// Grand total of all cash received.
    /// </summary>
    public decimal TotalEncaisse => EspecesPeriodes + EspecesVenteLubArticles + EspecesVenteServices + EspecesReglementCredits;

    /// <summary>
    /// Start date of the period for this summary.
    /// </summary>
    public DateTime? DateDebut { get; set; }

    /// <summary>
    /// End date of the period for this summary.
    /// </summary>
    public DateTime? DateFin { get; set; }
}
