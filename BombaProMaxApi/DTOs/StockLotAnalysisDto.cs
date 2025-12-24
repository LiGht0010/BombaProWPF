namespace BombaProMaxApi.DTOs;

/// <summary>
/// Analysis DTO for a single StockLot showing margin/profit data.
/// </summary>
public class StockLotAnalysisDto
{
    public int StockLotID { get; set; }
    public int AchatID { get; set; }
    public int ReservoirID { get; set; }
    public int ProduitID { get; set; }
    
    // Display fields
    public string? ReservoirNumero { get; set; }
    public string? ProduitNom { get; set; }
    public string? Statut { get; set; }
    public DateTime DateEntree { get; set; }

    // Quantities
    public decimal QuantiteInitiale { get; set; }
    public decimal QuantiteVendue { get; set; }
    public decimal QuantiteDisponible { get; set; }
    
    /// <summary>
    /// Percentage of lot sold
    /// </summary>
    public decimal PourcentageVendu => QuantiteInitiale > 0 
        ? Math.Round((QuantiteVendue / QuantiteInitiale) * 100, 2) 
        : 0;

    // Pricing
    public decimal PrixAchat { get; set; }
    public decimal PrixVenteMoyen { get; set; }

    // Financial metrics
    /// <summary>
    /// Total revenue from sales (Chiffre d'Affaires)
    /// </summary>
    public decimal ChiffreAffaires { get; set; }
    
    /// <summary>
    /// Cost of Goods Sold (Coût de Revient)
    /// </summary>
    public decimal CoutRevient { get; set; }
    
    /// <summary>
    /// Gross margin (Marge Brute) = Revenue - COGS
    /// </summary>
    public decimal MargeBrute => ChiffreAffaires - CoutRevient;
    
    /// <summary>
    /// Margin percentage
    /// </summary>
    public decimal MargePercent => ChiffreAffaires > 0 
        ? Math.Round((MargeBrute / ChiffreAffaires) * 100, 2) 
        : 0;

    /// <summary>
    /// Profit per liter sold
    /// </summary>
    public decimal MargeParLitre => QuantiteVendue > 0 
        ? Math.Round(MargeBrute / QuantiteVendue, 2) 
        : 0;
}

/// <summary>
/// Summary analysis for a reservoir showing aggregated margin data.
/// </summary>
public class ReservoirAnalysisDto
{
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    
    // Current state
    public decimal CapaciteTotale { get; set; }
    public decimal NiveauActuel { get; set; }
    public decimal PourcentageRempli => CapaciteTotale > 0 
        ? Math.Round((NiveauActuel / CapaciteTotale) * 100, 2) 
        : 0;

    // StockLot counts
    public int TotalStockLots { get; set; }
    public int StockLotsDisponibles { get; set; }
    public int StockLotsEpuises { get; set; }

    // Aggregated quantities
    public decimal TotalQuantiteAchetee { get; set; }
    public decimal TotalQuantiteVendue { get; set; }
    public decimal TotalQuantiteDisponible { get; set; }

    // Aggregated financials
    public decimal TotalChiffreAffaires { get; set; }
    public decimal TotalCoutRevient { get; set; }
    public decimal TotalMargeBrute => TotalChiffreAffaires - TotalCoutRevient;
    public decimal MargePercentGlobal => TotalChiffreAffaires > 0 
        ? Math.Round((TotalMargeBrute / TotalChiffreAffaires) * 100, 2) 
        : 0;

    // Price averages
    public decimal PrixAchatMoyen { get; set; }
    public decimal PrixVenteMoyen { get; set; }

    // Detailed lots
    public List<StockLotAnalysisDto> StockLots { get; set; } = [];
}

/// <summary>
/// Global summary analysis across all reservoirs.
/// </summary>
public class GlobalAnalysisSummaryDto
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }

    // Counts
    public int TotalReservoirs { get; set; }
    public int TotalStockLots { get; set; }
    public int TotalPeriodesAnalysees { get; set; }

    // Quantities
    public decimal TotalQuantiteAchetee { get; set; }
    public decimal TotalQuantiteVendue { get; set; }
    public decimal TotalQuantiteEnStock { get; set; }

    // Financials
    public decimal TotalChiffreAffaires { get; set; }
    public decimal TotalCoutRevient { get; set; }
    public decimal TotalMargeBrute => TotalChiffreAffaires - TotalCoutRevient;
    public decimal MargePercentGlobal => TotalChiffreAffaires > 0 
        ? Math.Round((TotalMargeBrute / TotalChiffreAffaires) * 100, 2) 
        : 0;

    // Per-product breakdown
    public List<ProductAnalysisSummaryDto> ParProduit { get; set; } = [];

    // Per-reservoir breakdown
    public List<ReservoirAnalysisDto> ParReservoir { get; set; } = [];
}

/// <summary>
/// Analysis summary per product (fuel type).
/// </summary>
public class ProductAnalysisSummaryDto
{
    public int ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    
    public decimal TotalQuantiteVendue { get; set; }
    public decimal TotalChiffreAffaires { get; set; }
    public decimal TotalCoutRevient { get; set; }
    public decimal TotalMargeBrute => TotalChiffreAffaires - TotalCoutRevient;
    public decimal MargePercent => TotalChiffreAffaires > 0 
        ? Math.Round((TotalMargeBrute / TotalChiffreAffaires) * 100, 2) 
        : 0;
}
