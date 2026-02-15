namespace BombaProMaxApi.DTOs;

/// <summary>
/// Request DTO for adjusting allocations when an Achat quantity is modified.
/// </summary>
public class AdjustAllocationsRequestDto
{
    /// <summary>
    /// The Achat ID being adjusted
    /// </summary>
    public int AchatId { get; set; }

    /// <summary>
    /// The new total quantity for the Achat
    /// </summary>
    public decimal NewAchatQuantite { get; set; }

    /// <summary>
    /// User performing the adjustment
    /// </summary>
    public string? UtilisateurAdjustment { get; set; }

    /// <summary>
    /// Optional notes for the adjustment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// The new allocation distribution (must sum to NewAchatQuantite)
    /// </summary>
    public List<AllocationAdjustmentItemDto> Allocations { get; set; } = [];
}

/// <summary>
/// Individual allocation adjustment item
/// </summary>
public class AllocationAdjustmentItemDto
{
    /// <summary>
    /// Existing allocation ID (0 if this is a new allocation)
    /// </summary>
    public int AllocationId { get; set; }

    /// <summary>
    /// Target reservoir ID
    /// </summary>
    public int ReservoirId { get; set; }

    /// <summary>
    /// The new quantity for this allocation
    /// </summary>
    public decimal NewQuantite { get; set; }
}

/// <summary>
/// Result of allocation adjustment validation
/// </summary>
public class AllocationAdjustmentValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<AllocationValidationDetailDto> Details { get; set; } = [];
}

/// <summary>
/// Validation detail for each allocation being adjusted
/// </summary>
public class AllocationValidationDetailDto
{
    public int AllocationId { get; set; }
    public int ReservoirId { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal OldQuantite { get; set; }
    public decimal NewQuantite { get; set; }
    public decimal Difference { get; set; }
    
    /// <summary>
    /// For decrease: the maximum amount that can be reduced based on QteRestante in reservoir
    /// </summary>
    public decimal MaxReducible { get; set; }
    
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Preview of current allocations for adjustment popup
/// </summary>
public class AdjustmentPreviewDto
{
    public int AchatId { get; set; }
    public string? AchatNumero { get; set; }
    public decimal CurrentAchatQuantite { get; set; }
    public int ProduitId { get; set; }
    public string? ProduitNom { get; set; }
    public decimal PrixAchatUnitaire { get; set; }
    public List<AllocationPreviewItemDto> Allocations { get; set; } = [];
}

/// <summary>
/// Preview of a single allocation for adjustment
/// </summary>
public class AllocationPreviewItemDto
{
    public int AllocationId { get; set; }
    public int ReservoirId { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal CurrentQuantite { get; set; }
    
    /// <summary>
    /// Amount already consumed from this allocation's StockLot (QteInitiale - QteRestante)
    /// </summary>
    public decimal ConsumedQuantite { get; set; }
    
    /// <summary>
    /// Maximum amount that can be reduced (CurrentQuantite - ConsumedQuantite limited by reservoir's total QteRestante)
    /// </summary>
    public decimal MaxReducible { get; set; }
    
    /// <summary>
    /// Total QteRestante available in this reservoir (across all lots)
    /// </summary>
    public decimal ReservoirQteRestante { get; set; }
    
    /// <summary>
    /// Reservoir capacity
    /// </summary>
    public decimal ReservoirCapacite { get; set; }
    
    /// <summary>
    /// Current reservoir level
    /// </summary>
    public decimal ReservoirNiveauActuel { get; set; }
}

/// <summary>
/// Result of allocation adjustment operation
/// </summary>
public class AllocationAdjustmentResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int AchatId { get; set; }
    public decimal OldAchatQuantite { get; set; }
    public decimal NewAchatQuantite { get; set; }
    public List<AchatAllocationDto> UpdatedAllocations { get; set; } = [];
}
