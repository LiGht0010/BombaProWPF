namespace BombaProMax.Models;

public class JaugeageDto
{
    public int ID { get; set; }
    public DateTime DateJaugeage { get; set; }
    public int TemoinID { get; set; }
    public string? NumeroJaugeage { get; set; }
    public string? Observations { get; set; }
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Display field for related entity
    public string? TemoinNom { get; set; }

    // Computed fields for list display
    public int DetailsCount { get; set; }
    public decimal TotalVolume { get; set; }
    
    // Comma-separated list of reservoir numbers included in this jaugeage
    public string? CiternesInclues { get; set; }
}

/// <summary>
/// DTO for creating/retrieving a Jaugeage with all its details in one operation
/// </summary>
public class JaugeageWithDetailsDto
{
    public int ID { get; set; }
    public DateTime DateJaugeage { get; set; }
    public int TemoinID { get; set; }
    public string? NumeroJaugeage { get; set; }
    public string? Observations { get; set; }
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Display field for related entity
    public string? TemoinNom { get; set; }

    // Details for each reservoir measured
    public List<JaugeageDetailDto> Details { get; set; } = new();

    // Computed properties
    public int DetailsCount => Details?.Count ?? 0;
    public decimal TotalVolume => Details?.Sum(d => d.VolumeCalcule) ?? 0;
    
    // Computed: Comma-separated list of reservoir numbers
    public string CiternesInclues => Details?.Count > 0 
        ? string.Join(", ", Details.Where(d => !string.IsNullOrEmpty(d.ReservoirNumero)).Select(d => d.ReservoirNumero))
        : "-";
}
