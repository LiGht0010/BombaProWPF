namespace BombaProMax.Models;

/// <summary>
/// DTO for creating a new period with all its details in a single operation.
/// </summary>
public class PeriodeWithDetailsDto
{
    public PeriodeDto Periode { get; set; } = new();
    public List<PeriodeDetailsDto> Details { get; set; } = [];
}
