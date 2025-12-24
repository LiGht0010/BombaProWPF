namespace BombaProMaxApi.DTOs;

public class FactureBonLivraisonDto
{
    public int ID { get; set; }
    public int FactureID { get; set; }
    public int BonLivraisonID { get; set; }
    public DateTime DateAssociation { get; set; }

    // Display fields
    public string? FactureNumero { get; set; }
    public string? BonLivraisonNumero { get; set; }
    public DateOnly? BonLivraisonDate { get; set; }
    public decimal? BonLivraisonMontant { get; set; }
}

public class CreateFactureFromBLsDto
{
    public List<int> BonLivraisonIds { get; set; } = new();
    public int ClientID { get; set; }
    public DateOnly? DateFacture { get; set; }
    public int? MoyenPaiementID { get; set; }
    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
}

public class FacturationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? FactureID { get; set; }
    public string? NumeroFacture { get; set; }
    public decimal? MontantTotal { get; set; }
    public int BLsFactures { get; set; }
    public List<string>? Errors { get; set; }
}
