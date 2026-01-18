namespace BombaProMaxApi.DTOs;

/// <summary>
/// Analysis DTO for a single StockLot showing margin/profit data.
///
/// MARGIN CALCULATION WARNING:
/// When PrixAchat = 0 (HasKnownCost = false), the margin calculations are inflated:
/// - CoutRevient will be 0
/// - MargeBrute will equal ChiffreAffaires (100% margin)
/// - MargePercent will be 100%
/// 
/// This typically occurs with OpeningBalance lots where the historical cost is unknown.
/// UI should flag these cases to avoid misleading financial reports.
/// </summary>
public class StockLotAnalysisDto
{
    public int StockLotID { get; set; }
    public int? AchatID { get; set; }
    public int ReservoirID { get; set; }
    public int ProduitID { get; set; }
    
    /// <summary>
    /// Type of stock lot: 0=OpeningBalance, 1=Purchase, 2=Adjustment
    /// </summary>
    public int Type { get; set; }
    
    /// <summary>
    /// Display name for the stock lot type
    /// </summary>
    public string TypeNom => Type switch
    {
        0 => "Stock Initial",
        1 => "Achat",
        2 => "Ajustement",
        _ => "Inconnu"
    };
    
    /// <summary>
    /// Indicates if this is an opening balance lot
    /// </summary>
    public bool IsOpeningBalance => Type == 0;
    
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
    
    /// <summary>
    /// Indicates if cost is known (PrixAchat > 0).
    /// Opening balance lots may have unknown cost (PrixAchat = 0).
    /// When false, margin calculations are unreliable and should be flagged in reports.
    /// </summary>
    public bool HasKnownCost => PrixAchat > 0;

    // Financial metrics
    /// <summary>
    /// Total revenue from sales (Chiffre d'Affaires)
    /// </summary>
    public decimal ChiffreAffaires { get; set; }
    
    /// <summary>
    /// Cost of Goods Sold (Coût de Revient).
    /// WARNING: When HasKnownCost = false (PrixAchat = 0), this will be 0,
    /// resulting in inflated margin calculations.
    /// </summary>
    public decimal CoutRevient { get; set; }
    
    /// <summary>
    /// Gross margin (Marge Brute) = Revenue - COGS.
    /// WARNING: When HasKnownCost = false, this equals ChiffreAffaires (artificially high).
    /// </summary>
    public decimal MargeBrute => ChiffreAffaires - CoutRevient;
    
    /// <summary>
    /// Margin percentage.
    /// WARNING: When HasKnownCost = false, this will be 100% (artificially high).
    /// Check HasKnownCost before relying on this value for financial decisions.
    /// </summary>
    public decimal MargePercent => ChiffreAffaires > 0 
        ? Math.Round((MargeBrute / ChiffreAffaires) * 100, 2) 
        : 0;

    /// <summary>
    /// Profit per liter sold.
    /// WARNING: When HasKnownCost = false, this equals PrixVenteMoyen (artificially high).
    /// </summary>
    public decimal MargeParLitre => QuantiteVendue > 0 
        ? Math.Round(MargeBrute / QuantiteVendue, 2) 
        : 0;
        
    /// <summary>
    /// Optional notes for the stock lot
    /// </summary>
    public string? Notes { get; set; }
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
    
    /// <summary>
    /// Global margin percentage.
    /// WARNING: May be inflated if any stock lots have HasKnownCost = false.
    /// Check HasUnknownCostStock before relying on this value.
    /// </summary>
    public decimal MargePercentGlobal => TotalChiffreAffaires > 0 
        ? Math.Round((TotalMargeBrute / TotalChiffreAffaires) * 100, 2) 
        : 0;

    // Price averages
    public decimal PrixAchatMoyen { get; set; }
    public decimal PrixVenteMoyen { get; set; }
    
    /// <summary>
    /// Indicates if any stock lot in this reservoir has unknown cost (PrixAchat = 0).
    /// When true, margin calculations may be inflated and should be flagged.
    /// </summary>
    public bool HasUnknownCostStock { get; set; }
    
    /// <summary>
    /// Count of stock lots with unknown cost (for reporting purposes)
    /// </summary>
    public int UnknownCostStockLotCount { get; set; }

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
    
    /// <summary>
    /// Global margin percentage.
    /// WARNING: May be inflated if any stock has unknown cost.
    /// Check HasUnknownCostStock before relying on this value.
    /// </summary>
    public decimal MargePercentGlobal => TotalChiffreAffaires > 0 
        ? Math.Round((TotalMargeBrute / TotalChiffreAffaires) * 100, 2) 
        : 0;
    
    /// <summary>
    /// Indicates if any stock in the analysis has unknown cost.
    /// When true, margin calculations may be inflated.
    /// </summary>
    public bool HasUnknownCostStock { get; set; }

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
