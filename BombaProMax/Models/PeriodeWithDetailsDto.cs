namespace BombaProMax.Models;

/// <summary>
/// DTO for creating a new period with all its details in a single operation.
/// </summary>
public class PeriodeWithDetailsDto
{
    public PeriodeDto Periode { get; set; } = new();
    public List<PeriodeDetailsDto> Details { get; set; } = [];
    
    /// <summary>
    /// List of CreditTransaction IDs to link to this période.
    /// These are carburant credit transactions that occurred during this shift.
    /// </summary>
    public List<int> CreditTransactionIds { get; set; } = [];
}
