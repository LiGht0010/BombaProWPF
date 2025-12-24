namespace BombaProMaxApi.DTOs
{
    /// <summary>
    /// Request DTO for batch fuel allocation to multiple reservoirs
    /// </summary>
    public class BatchAllocationRequestDto
    {
        /// <summary>
        /// The purchase (Achat) ID to allocate from
        /// </summary>
        public int AchatID { get; set; }

        /// <summary>
        /// Total quantity being allocated (for validation)
        /// </summary>
        public decimal TotalQuantite { get; set; }

        /// <summary>
        /// User performing the allocation
        /// </summary>
        public string? UtilisateurAllocation { get; set; }

        /// <summary>
        /// Optional notes for all allocations
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// List of individual reservoir allocations
        /// </summary>
        public List<AllocationItemDto> Allocations { get; set; } = [];
    }

    /// <summary>
    /// Individual allocation item for a single reservoir
    /// </summary>
    public class AllocationItemDto
    {
        /// <summary>
        /// Target reservoir ID
        /// </summary>
        public int ReservoirID { get; set; }

        /// <summary>
        /// Quantity to allocate to this reservoir (in liters)
        /// </summary>
        public decimal QuantiteAllouee { get; set; }

        /// <summary>
        /// Optional notes specific to this allocation
        /// </summary>
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
}
