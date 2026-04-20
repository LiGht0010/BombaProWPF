namespace BombaProMaxWPF.Models;

/// <summary>
/// Data model for Bon de Livraison PDF generation.
/// Contains all the data needed to generate a complete BL PDF report.
/// </summary>
public class BonLivraisonPdfData
{
    public int BonLivraisonID { get; set; }
    public string NumeroBL { get; set; } = "";
    public DateOnly DateBL { get; set; }
    public string? ClientNom { get; set; }
    public string? ClientNumero { get; set; }
    public string? ClientAdresse { get; set; }
    public string? ClientContact { get; set; }
    public bool EstFacture { get; set; }
    public string? Notes { get; set; }

    // Financial totals
    public decimal MontantTotal { get; set; }

    // Line items
    public List<BLDetailPdfData> Details { get; set; } = [];

    // Totals by product type
    public List<BLProduitTotalPdfData> TotauxParProduit { get; set; } = [];
}

/// <summary>
/// Line item data for BL PDF.
/// </summary>
public class BLDetailPdfData
{
    public string Description { get; set; } = "";
    public string? ProduitNom { get; set; }
    public string? ServiceNom { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal MontantLigne { get; set; }

    public string DisplayName => !string.IsNullOrEmpty(ProduitNom) ? ProduitNom :
                                 !string.IsNullOrEmpty(ServiceNom) ? ServiceNom :
                                 Description;
}

/// <summary>
/// Product totals summary for BL PDF.
/// </summary>
public class BLProduitTotalPdfData
{
    public string ProduitNom { get; set; } = "";
    public decimal QuantiteTotale { get; set; }
    public decimal MontantTotal { get; set; }
}
