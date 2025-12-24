namespace BombaProMaxApi.DTOs.Dashboard;

/// <summary>
/// Raw analytics row for Achat data.
/// Client-side will group/aggregate as needed for cards and detail views.
/// </summary>
public class AchatAnalyticsRowDto
{
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public string? CategorieNom { get; set; }
    public int Quantite { get; set; }
    public decimal PrixAchat { get; set; }
    public decimal PrixTotal { get; set; }
    public DateOnly DateAchat { get; set; }
}
