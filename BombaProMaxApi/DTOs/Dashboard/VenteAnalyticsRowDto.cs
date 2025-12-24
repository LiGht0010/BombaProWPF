namespace BombaProMaxApi.DTOs.Dashboard;

/// <summary>
/// Raw analytics row for Vente (Lubrifiants et Articles) data.
/// Client-side will group/aggregate as needed for cards and detail views.
/// </summary>
public class VenteAnalyticsRowDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int Quantite { get; set; }
    public decimal PrixVente { get; set; }
    public decimal PrixTotal { get; set; }
    public DateOnly DateVente { get; set; }
    public string? ClientNom { get; set; }
}
