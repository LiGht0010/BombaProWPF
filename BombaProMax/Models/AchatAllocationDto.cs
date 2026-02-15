namespace BombaProMax.Models
{
    public class AchatAllocationDto
    {
        public int ID { get; set; }
        public int AchatID { get; set; }
        public int ReservoirID { get; set; }
        public decimal QuantiteAllouee { get; set; }
        public DateTime DateAllocation { get; set; }
        public string? Notes { get; set; }
        public string Statut { get; set; } = "En Attente";
        public string? UtilisateurAllocation { get; set; }

        // Display fields for Achat
        public string? AchatNumero { get; set; }

        // Display fields for Reservoir
        public string? ReservoirNumero { get; set; }
        public decimal? ReservoirCapacite { get; set; }
        public decimal? ReservoirNiveauActuel { get; set; }
        public decimal? ReservoirEspaceDisponible => ReservoirCapacite - ReservoirNiveauActuel;

        // Display fields for Product (fuel type)
        public int? ProduitID { get; set; }
        public string? ProduitNom { get; set; }
    }

    /// <summary>
    /// Request DTO for batch fuel allocation to multiple reservoirs
    /// </summary>
    public class BatchAllocationRequestDto
    {
        public int AchatID { get; set; }
        public decimal TotalQuantite { get; set; }
        public string? UtilisateurAllocation { get; set; }
        public string? Notes { get; set; }
        public List<AllocationItemDto> Allocations { get; set; } = [];
    }

    /// <summary>
    /// Individual allocation item for a single reservoir
    /// </summary>
    public class AllocationItemDto
    {
        public int ReservoirID { get; set; }
        public decimal QuantiteAllouee { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response DTO for batch allocation result
    /// </summary>
    public class BatchAllocationResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int AchatID { get; set; }
        public decimal TotalAlloue { get; set; }
        public List<AchatAllocationDto> Allocations { get; set; } = [];
    }

    /// <summary>
    /// DTO for reservoir availability information during allocation
    /// </summary>
    public class ReservoirAllocationInfoDto
    {
        public int ID { get; set; }
        public string Numero { get; set; } = null!;
        public int? ProduitID { get; set; }
        public string? ProduitNom { get; set; }
        public decimal Capacite { get; set; }
        public decimal NiveauActuel { get; set; }
        public decimal EspaceDisponible => Capacite - NiveauActuel;
        public decimal TauxRemplissage => Capacite > 0 ? Math.Round((NiveauActuel / Capacite) * 100, 1) : 0;
        public bool EstVide => ProduitID == null || NiveauActuel == 0;
        public bool EstCompatible { get; set; }
    }

    /// <summary>
    /// DTO for checking achat allocation status
    /// </summary>
    public class AchatAllocationStatusDto
    {
        public int AchatID { get; set; }
        public int Quantite { get; set; }
        public decimal TotalAlloue { get; set; }
        public decimal Restant { get; set; }
        public bool EstCompletementAlloue { get; set; }
        public bool EstCarburant { get; set; }
        public int? ProduitID { get; set; }
        public string? ProduitNom { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ALLOCATION ADJUSTMENT DTOs
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Request DTO for adjusting allocations when an Achat quantity is modified.
    /// </summary>
    public class AdjustAllocationsRequestDto
    {
        public int AchatId { get; set; }
        public decimal NewAchatQuantite { get; set; }
        public string? UtilisateurAdjustment { get; set; }
        public string? Notes { get; set; }
        public List<AllocationAdjustmentItemDto> Allocations { get; set; } = [];
    }

    /// <summary>
    /// Individual allocation adjustment item
    /// </summary>
    public class AllocationAdjustmentItemDto
    {
        public int AllocationId { get; set; }
        public int ReservoirId { get; set; }
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
        public decimal ConsumedQuantite { get; set; }
        public decimal MaxReducible { get; set; }
        public decimal ReservoirQteRestante { get; set; }
        public decimal ReservoirCapacite { get; set; }
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
}