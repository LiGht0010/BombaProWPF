namespace FourniPro.Models;

public class StockVoyageDto
{
    public int StockVoyageId { get; set; }
    public int VoyageId { get; set; }

    public int? ProduitId { get; set; }
    public string? ProduitNom { get; set; }

    public int? Quantite { get; set; }
}
