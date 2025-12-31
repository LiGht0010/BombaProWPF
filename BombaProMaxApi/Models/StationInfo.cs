using System.ComponentModel.DataAnnotations;

namespace BombaProMaxApi.Models;

/// <summary>
/// Station information entity for storing business details used in official documents.
/// This is a singleton table - only one record should exist per tenant.
/// </summary>
public class StationInfo
{
    [Key]
    public int ID { get; set; }
    
    /// <summary>
    /// Station/Company name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string StationName { get; set; } = "";
    
    /// <summary>
    /// Physical address
    /// </summary>
    [MaxLength(500)]
    public string? Adresse { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    [MaxLength(100)]
    public string? Ville { get; set; }
    
    /// <summary>
    /// Taxe Professionnelle number
    /// </summary>
    [MaxLength(50)]
    public string? TP { get; set; }
    
    /// <summary>
    /// Identifiant Fiscal
    /// </summary>
    [MaxLength(50)]
    public string? IF { get; set; }
    
    /// <summary>
    /// Registre de Commerce
    /// </summary>
    [MaxLength(50)]
    public string? RC { get; set; }
    
    /// <summary>
    /// CNSS number
    /// </summary>
    [MaxLength(50)]
    public string? CNSS { get; set; }
    
    /// <summary>
    /// Identifiant Commun de l'Entreprise
    /// </summary>
    [MaxLength(50)]
    public string? ICE { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    [MaxLength(30)]
    public string? Tel { get; set; }
    
    /// <summary>
    /// Fax number
    /// </summary>
    [MaxLength(30)]
    public string? Fax { get; set; }
    
    /// <summary>
    /// Website URL
    /// </summary>
    [MaxLength(200)]
    public string? SiteWeb { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Logo stored as binary data
    /// </summary>
    public byte[]? Logo { get; set; }
    
    /// <summary>
    /// Date of last modification
    /// </summary>
    public DateTime? DateModification { get; set; }
}
