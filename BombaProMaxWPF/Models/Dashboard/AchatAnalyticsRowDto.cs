namespace BombaProMaxWPF.Models.Dashboard;

/// <summary>
/// Raw analytics row for Achat data.
/// Used for grouping into cards and displaying detail tables.
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
