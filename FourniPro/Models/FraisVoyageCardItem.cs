namespace FourniPro.Models;

public class FraisVoyageCardItem
{
    public int FraisVoyageId { get; set; }
    public int VoyageId { get; set; }
    public string? Type { get; set; }
    public decimal? Montant { get; set; }
    public string? Description { get; set; }
}
