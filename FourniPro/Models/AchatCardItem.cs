namespace FourniPro.Models;

public class AchatCardItem
{
    public int AchatId { get; set; }
    public string? Numero { get; set; }
    public DateOnly Date { get; set; }
    public string? FournisseurNom { get; set; }
    public string? ProduitNom { get; set; }
    public int? Quantite { get; set; }
    public decimal? Cout { get; set; }
    public bool? LivraisonDefectueuse { get; set; }
}
