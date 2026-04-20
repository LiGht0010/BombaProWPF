namespace BombaProMaxWPF.Models;

/// <summary>
/// Data transfer object for station information used in official documents (factures, BLs, etc.)
/// </summary>
public class StationInfoDto
{
    public int ID { get; set; }
    
    /// <summary>
    /// Station/Company name
    /// </summary>
    public string StationName { get; set; } = "";
    
    /// <summary>
    /// Physical address
    /// </summary>
    public string? Adresse { get; set; }
    
    /// <summary>
    /// Taxe Professionnelle number
    /// </summary>
    public string? TP { get; set; }
    
    /// <summary>
    /// Identifiant Fiscal
    /// </summary>
    public string? IF { get; set; }
    
    /// <summary>
    /// Registre de Commerce
    /// </summary>
    public string? RC { get; set; }
    
    /// <summary>
    /// CNSS number
    /// </summary>
    public string? CNSS { get; set; }
    
    /// <summary>
    /// Identifiant Commun de l'Entreprise
    /// </summary>
    public string? ICE { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string? Tel { get; set; }
    
    /// <summary>
    /// Fax number
    /// </summary>
    public string? Fax { get; set; }
    
    /// <summary>
    /// Website URL
    /// </summary>
    public string? SiteWeb { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Logo as base64 encoded string (for API transport)
    /// </summary>
    public string? LogoBase64 { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    public string? Ville { get; set; }
}
