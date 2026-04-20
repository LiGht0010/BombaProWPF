namespace BombaProMaxWPF.Models;

/// <summary>
/// Filter parameters for reports.
/// </summary>
public class RapportFilterDto
{
    public DateOnly? DateSpecifique { get; set; }
    public int? Mois { get; set; }
    public int? Annee { get; set; }

    public string? MoisAnnee => Mois.HasValue && Annee.HasValue
        ? $"{Annee.Value}-{Mois.Value:D2}"
        : null;
}

/// <summary>
/// Sales report summary data.
/// </summary>
public class RapportVentesDto
{
    // Ventes Carburant (from Periode/PeriodeDetails)
    public decimal TotalVentesCarburant { get; set; }
    public decimal TotalQuantiteCarburant { get; set; }
    public List<RapportVenteCarburantProduitDto> VentesCarburantParProduit { get; set; } = [];

    // Ventes Lubrifiants & Articles
    public decimal TotalVentesLubArticles { get; set; }
    public int TotalQuantiteLubArticles { get; set; }
    public List<RapportVenteLubArticleProduitDto> VentesLubArticlesParProduit { get; set; } = [];

    // Ventes Services
    public decimal TotalVentesServices { get; set; }
    public int TotalQuantiteServices { get; set; }
    public List<RapportVenteServiceDto> VentesServicesParService { get; set; } = [];

    // Grand Total
    public decimal TotalVentes => TotalVentesCarburant + TotalVentesLubArticles + TotalVentesServices;
}

/// <summary>
/// Fuel sales grouped by product.
/// </summary>
public class RapportVenteCarburantProduitDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public decimal TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombrePeriodes { get; set; }
}

/// <summary>
/// Lubricants/Articles sales grouped by product.
/// </summary>
public class RapportVenteLubArticleProduitDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreVentes { get; set; }
}

/// <summary>
/// Service sales grouped by service.
/// </summary>
public class RapportVenteServiceDto
{
    public int ServiceId { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreVentes { get; set; }
}

/// <summary>
/// Expenses report summary data.
/// </summary>
public class RapportDepensesDto
{
    public decimal TotalDepenses { get; set; }
    public int NombreDepenses { get; set; }
    public List<RapportDepenseCategorieDto> DepensesParCategorie { get; set; } = [];
    public List<RapportDepenseDetailDto> DepensesDetails { get; set; } = [];
}

/// <summary>
/// Expenses grouped by category.
/// </summary>
public class RapportDepenseCategorieDto
{
    public string CategorieNom { get; set; } = string.Empty;
    public decimal TotalMontant { get; set; }
    public int NombreDepenses { get; set; }
}

/// <summary>
/// Individual expense detail for the table.
/// </summary>
public class RapportDepenseDetailDto
{
    public int Id { get; set; }
    public string? Numero { get; set; }
    public DateOnly? Date { get; set; }
    public string? Categorie { get; set; }
    public decimal Montant { get; set; }
    public string? Description { get; set; }

    public string DateDisplay => Date?.ToString("dd/MM/yyyy") ?? "-";
}

/// <summary>
/// Stock report summary data.
/// </summary>
public class RapportStockDto
{
    // Stock Carburant (Reservoirs)
    public List<RapportStockReservoirDto> StockCarburant { get; set; } = [];
    public decimal TotalStockCarburantLitres { get; set; }

    // Stock Produits (Non-Carburant)
    public List<RapportStockProduitDto> StockProduits { get; set; } = [];
    public int TotalStockProduits { get; set; }

    // Mouvements (Achats during period)
    public decimal TotalAchatsPeriode { get; set; }
    public List<RapportAchatProduitDto> AchatsParProduit { get; set; } = [];
}

/// <summary>
/// Reservoir stock data.
/// </summary>
public class RapportStockReservoirDto
{
    public int ReservoirId { get; set; }
    public string ReservoirNumero { get; set; } = string.Empty;
    public string? ProduitNom { get; set; }
    public decimal Capacite { get; set; }
    public decimal NiveauActuel { get; set; }
    public decimal PourcentageRemplissage => Capacite > 0 ? (NiveauActuel / Capacite) * 100 : 0;
}

/// <summary>
/// Product stock data (non-fuel).
/// </summary>
public class RapportStockProduitDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int StockActuel { get; set; }
    public int? StockMinimum { get; set; }
    public bool IsLowStock => StockMinimum.HasValue && StockActuel <= StockMinimum.Value;
}

/// <summary>
/// Purchases grouped by product for stock movements.
/// </summary>
public class RapportAchatProduitDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public decimal TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
    public int NombreAchats { get; set; }
}

/// <summary>
/// Combined report response from API.
/// </summary>
public class RapportCompletDto
{
    public RapportVentesDto Ventes { get; set; } = new();
    public RapportDepensesDto Depenses { get; set; } = new();
    public RapportStockDto Stock { get; set; } = new();
    public string PeriodeLabel { get; set; } = string.Empty;
}

/// <summary>
/// Jaugeage analysis response comparing last 2 jaugeages vs sales.
/// </summary>
public class RapportJaugeageAnalyseDto
{
    public bool HasData { get; set; }
    public string? Message { get; set; }
    public RapportJaugeageInfoDto? JaugeageActuel { get; set; }
    public RapportJaugeageInfoDto? JaugeagePrecedent { get; set; }
    public string? PeriodeAnalyse { get; set; }
    public List<RapportJaugeageReservoirComparisonDto> Comparaisons { get; set; } = [];
}

/// <summary>
/// Jaugeage summary info.
/// </summary>
public class RapportJaugeageInfoDto
{
    public int Id { get; set; }
    public string? NumeroJaugeage { get; set; }
    public DateTime DateJaugeage { get; set; }
    public string? TemoinNom { get; set; }

    public string DateDisplay => DateJaugeage.ToString("dd/MM/yyyy HH:mm");
}

/// <summary>
/// Per-reservoir comparison between jaugeage stock consumption and actual sales.
/// </summary>
public class RapportJaugeageReservoirComparisonDto
{
    public int ReservoirId { get; set; }
    public string ReservoirNumero { get; set; } = string.Empty;
    public string? ProduitNom { get; set; }
    
    /// <summary>Volume from previous jaugeage (n-1)</summary>
    public decimal VolumePrecedent { get; set; }
    
    /// <summary>Volume from current jaugeage (n)</summary>
    public decimal VolumeActuel { get; set; }
    
    /// <summary>Stock consumed according to jaugeage = VolumePrecedent - VolumeActuel</summary>
    public decimal StockConsomme { get; set; }
    
    /// <summary>Quantity sold from PeriodeDetails between the 2 jaugeages</summary>
    public decimal QuantiteVendue { get; set; }
    
    /// <summary>Difference = StockConsomme - QuantiteVendue</summary>
    public decimal Ecart { get; set; }
    
    /// <summary>Ecart as percentage of QuantiteVendue</summary>
    public decimal EcartPourcentage { get; set; }
    
    /// <summary>Status: "Normal", "Remise au cuve", "Manquant"</summary>
    public string Statut { get; set; } = "Normal";

    /// <summary>Color code for UI based on status</summary>
    public string StatutColor => Statut switch
    {
        "Normal" => "#2E7D32",      // Green
        "Remise au cuve" => "#FF9800", // Orange
        "Manquant" => "#C62828",    // Red
        _ => "#666666"
    };

    /// <summary>Background color for UI based on status</summary>
    public string StatutBackgroundColor => Statut switch
    {
        "Normal" => "#E8F5E9",      // Light green
        "Remise au cuve" => "#FFF3E0", // Light orange
        "Manquant" => "#FFEBEE",    // Light red
        _ => "#F5F5F5"
    };
}
