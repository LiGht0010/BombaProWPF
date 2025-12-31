namespace BombaProMax.Models;

/// <summary>
/// Data model for Facture PDF generation.
/// Contains all the data needed to generate a complete Facture PDF report.
/// </summary>
public class FacturePdfData
{
    public int FactureID { get; set; }
    public string NumeroFacture { get; set; } = "";
    public DateOnly DateFacture { get; set; }
    
    // Client information
    public string? ClientNom { get; set; }
    public string? ClientNumero { get; set; }
    public string? ClientAdresse { get; set; }
    public string? ClientContact { get; set; }
    public string? ClientICE { get; set; }
    public string? ClientIF { get; set; }
    
    // Facture status
    public string? Statut { get; set; }
    public DateOnly? DatePaiement { get; set; }
    public string? MoyenPaiementNom { get; set; }
    
    // Payment conditions
    public string? ConditionsPaiement { get; set; }
    public int? DelaiPaiementJours { get; set; }

    // Financial totals
    public decimal MontantTotal { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal TauxTVA { get; set; } = 20; // Default 20%

    // Line items
    public List<FactureElementPdfData> Elements { get; set; } = [];

    // Linked BLs (if any)
    public List<FactureBLLinkPdfData> BonsLivraisonLies { get; set; } = [];
    
    // Station information for header
    public StationInfoDto? StationInfo { get; set; }
}

/// <summary>
/// Line item data for Facture PDF.
/// </summary>
public class FactureElementPdfData
{
    public string Description { get; set; } = "";
    public string? ProduitNom { get; set; }
    public string? ServiceNom { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal MontantLigne => Quantite * PrixUnitaire;

    public string DisplayName => !string.IsNullOrEmpty(ProduitNom) ? ProduitNom :
                                 !string.IsNullOrEmpty(ServiceNom) ? ServiceNom :
                                 Description;
}

/// <summary>
/// Linked BL summary for Facture PDF.
/// </summary>
public class FactureBLLinkPdfData
{
    public int BLID { get; set; }
    public string NumeroBL { get; set; } = "";
    public DateOnly DateBL { get; set; }
    public decimal MontantBL { get; set; }
}
