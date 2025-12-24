using System.Collections.Generic;

namespace BombaProMaxApi.DTOs;

/// <summary>
/// DTO for creating a Periode with all its PeriodeDetails in a single atomic operation.
/// This is the primary entry point for sales recording - triggers stock consumption.
/// </summary>
public class PeriodeWithDetailsDto
{
    public PeriodeDto Periode { get; set; } = new();
    public List<PeriodeDetailsDto> Details { get; set; } = [];
}
