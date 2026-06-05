namespace FourniPro.Models;

public class VenteCardItem
{
    public int VenteId { get; set; }
    public string? NumeroVente { get; set; }
    public DateOnly DateVente { get; set; }
    public string? ClientNom { get; set; }
    public string? ProduitNom { get; set; }
    public string? EmployeNom { get; set; }
    public int? VoyageID { get; set; }
    public string? VoyageNumero { get; set; }
    public int? Quantite { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? MontantTotal { get; set; }
    public string? PaymentMethod { get; set; }
}
