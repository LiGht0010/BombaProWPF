namespace BombaProMaxApi.DTOs;

public class BonLivraisonDetailsDto
{
    public int ID { get; set; }
    public int BonLivraisonID { get; set; }
    public int? ProduitID { get; set; }
    public int? ServiceID { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal MontantLigne { get; set; }
    public string? Description { get; set; }

    // Display fields
    public string? ProduitNom { get; set; }
    public string? ProduitNumero { get; set; }
    public string? ServiceNom { get; set; }
    public string? ServiceNumero { get; set; }
}

public class CreateBonLivraisonDetailsDto
{
    public int? ProduitID { get; set; }
    public int? ServiceID { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string? Description { get; set; }
}
