namespace FourniPro.Models;

public class StockVoyageCardItem
{
    public int StockVoyageId { get; set; }
    public int VoyageId { get; set; }
    public string? ProduitNom { get; set; }
    public int? Quantite { get; set; }
}
