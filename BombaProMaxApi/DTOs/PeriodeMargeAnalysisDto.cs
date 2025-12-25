namespace BombaProMaxApi.DTOs;

/// <summary>
/// DTO for stock consumption analysis by Periode.
/// Shows which StockLots were consumed, at what price, and the resulting margin.
/// </summary>
public class PeriodeMargeAnalysisDto
{
    public int PeriodeID { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    
    /// <summary>
    /// Individual consumption records showing FIFO breakdown
    /// </summary>
    public List<ConsommationDetailDto> Consommations { get; set; } = [];
    
    /// <summary>
    /// Aggregated totals by product
    /// </summary>
    public List<ProduitMargeDto> ParProduit { get; set; } = [];
    
    // Totals
    public decimal TotalQuantiteVendue { get; set; }
    public decimal TotalCoutAchat { get; set; }
    public decimal TotalVente { get; set; }
    public decimal TotalMarge => TotalVente - TotalCoutAchat;
    public decimal MargePercent => TotalVente > 0 
        ? Math.Round((TotalMarge / TotalVente) * 100, 2) 
        : 0;
}

/// <summary>
/// Individual stock lot consumption detail for a PeriodeDetail
/// </summary>
public class ConsommationDetailDto
{
    public int ConsumptionID { get; set; }
    public int StockLotID { get; set; }
    public int PeriodeDetailID { get; set; }
    
    // Product info
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    
    // Reservoir info
    public int? ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    
    // Pump info
    public int? PompeID { get; set; }
    public string? PompeNumero { get; set; }
    
    /// <summary>
    /// Quantity consumed from this specific StockLot
    /// </summary>
    public decimal QuantiteConsommee { get; set; }
    
    /// <summary>
    /// Purchase price per liter (from StockLot - FIFO cost)
    /// </summary>
    public decimal PrixAchat { get; set; }
    
    /// <summary>
    /// Selling price per liter (from PeriodeDetail)
    /// </summary>
    public decimal PrixVente { get; set; }
    
    /// <summary>
    /// Cost of goods sold (Quantite * PrixAchat)
    /// </summary>
    public decimal CoutAchat => Math.Round(QuantiteConsommee * PrixAchat, 2);
    
    /// <summary>
    /// Revenue (Quantite * PrixVente)
    /// </summary>
    public decimal Vente => Math.Round(QuantiteConsommee * PrixVente, 2);
    
    /// <summary>
    /// Gross margin for this consumption
    /// </summary>
    public decimal Marge => Vente - CoutAchat;
    
    /// <summary>
    /// Margin per liter
    /// </summary>
    public decimal MargeParLitre => PrixVente - PrixAchat;
    
    /// <summary>
    /// Margin percentage
    /// </summary>
    public decimal MargePercent => Vente > 0 
        ? Math.Round((Marge / Vente) * 100, 2) 
        : 0;
    
    /// <summary>
    /// Date when consumption occurred
    /// </summary>
    public DateTime DateConsommation { get; set; }
}

/// <summary>
/// Aggregated margin data per product for a Periode
/// </summary>
public class ProduitMargeDto
{
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    
    public decimal TotalQuantite { get; set; }
    public decimal TotalCoutAchat { get; set; }
    public decimal TotalVente { get; set; }
    public decimal TotalMarge => TotalVente - TotalCoutAchat;
    public decimal MargePercent => TotalVente > 0 
        ? Math.Round((TotalMarge / TotalVente) * 100, 2) 
        : 0;
    
    /// <summary>
    /// Average purchase price per liter
    /// </summary>
    public decimal PrixAchatMoyen { get; set; }
    
    /// <summary>
    /// Average selling price per liter
    /// </summary>
    public decimal PrixVenteMoyen { get; set; }
}
