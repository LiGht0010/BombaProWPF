namespace BombaProMax.Models.Dashboard;

/// <summary>
/// Client-side model for displaying product cards in dashboard.
/// Created by grouping AchatAnalyticsRowDto or VenteAnalyticsRowDto.
/// </summary>
public class ProductCardModel
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int TotalQuantite { get; set; }
    public decimal TotalMontant { get; set; }
}
