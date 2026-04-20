namespace BombaProMaxWPF.Models;

/// <summary>
/// DTO for creating an Opening Balance StockLot.
/// Used during onboarding when a reservoir has existing fuel.
/// </summary>
public class OpeningBalanceCreateDto
{
    /// <summary>
    /// The reservoir ID where the initial stock resides
    /// </summary>
    public int ReservoirID { get; set; }

    /// <summary>
    /// The product/fuel type ID
    /// </summary>
    public int ProduitID { get; set; }

    /// <summary>
    /// Initial quantity in liters (must be > 0)
    /// </summary>
    public decimal Quantite { get; set; }

    /// <summary>
    /// Estimated purchase price per liter (can be 0 if unknown)
    /// </summary>
    public decimal PrixAchat { get; set; } = 0;

    /// <summary>
    /// Date when stock entered the reservoir (defaults to now if not provided)
    /// </summary>
    public DateTime? DateEntree { get; set; }

    /// <summary>
    /// Optional notes explaining the opening balance
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response DTO after creating an Opening Balance StockLot.
/// </summary>
public class OpeningBalanceResultDto
{
    public int StockLotID { get; set; }
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public int ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixAchat { get; set; }
    public DateTime DateEntree { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Indicates if cost is known (PrixAchat > 0)
    /// </summary>
    public bool HasKnownCost => PrixAchat > 0;
    
    /// <summary>
    /// New reservoir level after opening balance
    /// </summary>
    public decimal NouveauNiveau { get; set; }
    
    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO for checking reservoir stock status before creating opening balance.
/// </summary>
public class ReservoirStockStatusDto
{
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    public decimal Capacite { get; set; }
    public decimal NiveauActuel { get; set; }
    
    /// <summary>
    /// True if reservoir already has stock lots
    /// </summary>
    public bool HasStockLots { get; set; }
    
    /// <summary>
    /// True if reservoir already has an opening balance
    /// </summary>
    public bool HasOpeningBalance { get; set; }
    
    /// <summary>
    /// True if opening balance can be created (no existing stock lots)
    /// </summary>
    public bool CanCreateOpeningBalance => !HasStockLots;
    
    /// <summary>
    /// Reason why opening balance cannot be created (if applicable)
    /// </summary>
    public string? BlockingReason { get; set; }
}
