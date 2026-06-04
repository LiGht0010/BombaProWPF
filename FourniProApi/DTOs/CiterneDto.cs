namespace FourniProApi.DTOs;

/// <summary>
/// Used for both GET responses and POST/PUT request bodies.
/// </summary>
public class CiterneDto
{
    public int CiterneId { get; set; }
    public string? MatriculeCiterne { get; set; }
    public decimal? Capacite { get; set; }
    public int? PartitionsNumber { get; set; }

    // Audit
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
