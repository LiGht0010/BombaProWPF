namespace BombaProMaxWPF.Models;

/// <summary>
/// Data model for Rapport PDF generation.
/// Contains all data needed to generate a complete multi-section PDF report.
/// </summary>
public class RapportPdfData
{
    /// <summary>Period label shown in header (e.g., "Janvier 2024" or "15/01/2024")</summary>
    public string PeriodeLabel { get; set; } = string.Empty;

    /// <summary>Date when the report was generated</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    // ???????????????????????????????????????????????????????????????
    // VENTES SECTION
    // ???????????????????????????????????????????????????????????????
    public RapportVentesPdfData Ventes { get; set; } = new();

    // ???????????????????????????????????????????????????????????????
    // DEPENSES SECTION
    // ???????????????????????????????????????????????????????????????
    public RapportDepensesPdfData Depenses { get; set; } = new();

    // ???????????????????????????????????????????????????????????????
    // STOCK SECTION
    // ???????????????????????????????????????????????????????????????
    public RapportStockPdfData Stock { get; set; } = new();
}

/// <summary>
/// Ventes (Sales) section data for PDF.
/// </summary>
public class RapportVentesPdfData
{
    // Summary totals
    public decimal TotalVentes { get; set; }
    public decimal TotalVentesCarburant { get; set; }
    public decimal TotalQuantiteCarburant { get; set; }
    public decimal TotalVentesLubArticles { get; set; }
    public int TotalQuantiteLubArticles { get; set; }
    public decimal TotalVentesServices { get; set; }
    public int TotalQuantiteServices { get; set; }

    // Detailed tables
    public List<RapportVenteCarburantProduitPdfData> VentesCarburantParProduit { get; set; } = [];
    public List<RapportVenteLubArticleProduitPdfData> VentesLubArticlesParProduit { get; set; } = [];
    public List<RapportVenteServicePdfData> VentesServicesParService { get; set; } = [];
}

/// <summary>
/// Fuel sales by product for PDF.
/// </summary>
public class RapportVenteCarburantProduitPdfData
{
    public string ProduitNom { get; set; } = string.Empty;
    public decimal TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombrePeriodes { get; set; }
}

/// <summary>
/// Lubricants/Articles sales by product for PDF.
/// </summary>
public class RapportVenteLubArticleProduitPdfData
{
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreVentes { get; set; }
}

/// <summary>
/// Service sales by service for PDF.
/// </summary>
public class RapportVenteServicePdfData
{
    public string ServiceDescription { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreVentes { get; set; }
}

/// <summary>
/// Dépenses (Expenses) section data for PDF.
/// </summary>
public class RapportDepensesPdfData
{
    // Summary totals
    public decimal TotalDepenses { get; set; }
    public int NombreDepenses { get; set; }

    // Grouped by category
    public List<RapportDepenseCategoriePdfData> DepensesParCategorie { get; set; } = [];

    // Detailed list
    public List<RapportDepenseDetailPdfData> DepensesDetails { get; set; } = [];
}

/// <summary>
/// Expenses grouped by category for PDF.
/// </summary>
public class RapportDepenseCategoriePdfData
{
    public string CategorieNom { get; set; } = string.Empty;
    public decimal TotalMontant { get; set; }
    public int NombreDepenses { get; set; }
}

/// <summary>
/// Individual expense detail for PDF.
/// </summary>
public class RapportDepenseDetailPdfData
{
    public string? Numero { get; set; }
    public string DateDisplay { get; set; } = "-";
    public string? Categorie { get; set; }
    public decimal Montant { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Stock section data for PDF.
/// </summary>
public class RapportStockPdfData
{
    // Summary totals
    public decimal TotalStockCarburantLitres { get; set; }
    public int TotalStockProduits { get; set; }
    public decimal TotalAchatsPeriode { get; set; }

    // Reservoir stock
    public List<RapportStockReservoirPdfData> StockCarburant { get; set; } = [];

    // Product stock (non-fuel)
    public List<RapportStockProduitPdfData> StockProduits { get; set; } = [];

    // Purchases during period
    public List<RapportAchatProduitPdfData> AchatsParProduit { get; set; } = [];

    // Jaugeage analysis
    public RapportJaugeageAnalysePdfData JaugeageAnalyse { get; set; } = new();
}

/// <summary>
/// Reservoir stock data for PDF.
/// </summary>
public class RapportStockReservoirPdfData
{
    public string ReservoirNumero { get; set; } = string.Empty;
    public string? ProduitNom { get; set; }
    public decimal Capacite { get; set; }
    public decimal NiveauActuel { get; set; }
    public decimal PourcentageRemplissage { get; set; }
}

/// <summary>
/// Product stock data (non-fuel) for PDF.
/// </summary>
public class RapportStockProduitPdfData
{
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int StockActuel { get; set; }
    public int? StockMinimum { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// Purchases by product for PDF.
/// </summary>
public class RapportAchatProduitPdfData
{
    public string ProduitNom { get; set; } = string.Empty;
    public decimal TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreAchats { get; set; }
}

/// <summary>
/// Jaugeage analysis section for PDF.
/// </summary>
public class RapportJaugeageAnalysePdfData
{
    public bool HasData { get; set; }
    public string? Message { get; set; }
    public string? PeriodeAnalyse { get; set; }
    public string? JaugeagePrecedentInfo { get; set; }
    public string? JaugeageActuelInfo { get; set; }

    public List<RapportJaugeageComparisonPdfData> Comparaisons { get; set; } = [];
}

/// <summary>
/// Per-reservoir jaugeage comparison for PDF.
/// </summary>
public class RapportJaugeageComparisonPdfData
{
    public string ReservoirNumero { get; set; } = string.Empty;
    public string? ProduitNom { get; set; }
    public decimal VolumePrecedent { get; set; }
    public decimal VolumeActuel { get; set; }
    public decimal StockConsomme { get; set; }
    public decimal QuantiteVendue { get; set; }
    public decimal Ecart { get; set; }

    /// <summary>Status: "Normal", "Remise au cuve", "Manquant"</summary>
    public string Statut { get; set; } = "Normal";

    /// <summary>Color for PDF based on status</summary>
    public string StatutColor => Statut switch
    {
        "Normal" => "#2E7D32",       // Green
        "Remise au cuve" => "#FF9800", // Orange
        "Manquant" => "#C62828",     // Red
        _ => "#666666"
    };
}
