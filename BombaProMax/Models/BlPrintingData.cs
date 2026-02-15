namespace BombaProMax.Models;

/// <summary>
/// Data model for Bon de Livraison printing.
/// Simplified structure with only Quantite and Produit (no prices).
/// Contains all the data needed to generate a complete BL print document.
/// </summary>
public class BlPrintingData
{
    public int BonLivraisonID { get; set; }
    public string NumeroBL { get; set; } = "";
    public DateOnly DateBL { get; set; }
    
    // Client information
    public string? ClientNom { get; set; }
    public string? ClientNumero { get; set; }
    public string? ClientAdresse { get; set; }
    public string? ClientContact { get; set; }
    public string? ClientICE { get; set; }
    public string? ClientIF { get; set; }
    
    // BL status
    public bool EstFacture { get; set; }
    public string? Notes { get; set; }

    // Totals (quantity only, no financial)
    public decimal TotalQuantite { get; set; }
    public int NombreElements { get; set; }

    // Line items (simplified - only Qte and Produit)
    public List<BlPrintingElement> Elements { get; set; } = [];

    // Totals by product type (simplified)
    public List<BlPrintingProduitTotal> TotauxParProduit { get; set; } = [];
    
    // Station information for header
    public StationInfoDto? StationInfo { get; set; }
}

/// <summary>
/// Line item data for BL printing.
/// Simplified to only include Produit and Quantite.
/// </summary>
public class BlPrintingElement
{
    public string? ProduitNom { get; set; }
    public string? ServiceNom { get; set; }
    public string? Description { get; set; }
    public decimal Quantite { get; set; }

    /// <summary>
    /// Display name prioritizing ProduitNom, then ServiceNom, then Description.
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(ProduitNom) ? ProduitNom :
                                 !string.IsNullOrEmpty(ServiceNom) ? ServiceNom :
                                 Description ?? "";
}

/// <summary>
/// Product totals summary for BL printing.
/// Simplified to only include Produit and Quantite.
/// </summary>
public class BlPrintingProduitTotal
{
    public string ProduitNom { get; set; } = "";
    public decimal QuantiteTotale { get; set; }
}
