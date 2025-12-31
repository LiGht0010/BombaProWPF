namespace BombaProMaxApi.DTOs;

/// <summary>
/// Data transfer object for station information.
/// </summary>
public class StationInfoDto
{
    public int ID { get; set; }
    public string StationName { get; set; } = "";
    public string? Adresse { get; set; }
    public string? Ville { get; set; }
    public string? TP { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? CNSS { get; set; }
    public string? ICE { get; set; }
    public string? Tel { get; set; }
    public string? Fax { get; set; }
    public string? SiteWeb { get; set; }
    public string? Email { get; set; }
    
    /// <summary>
    /// Logo as base64 encoded string (for API transport)
    /// </summary>
    public string? LogoBase64 { get; set; }
}
