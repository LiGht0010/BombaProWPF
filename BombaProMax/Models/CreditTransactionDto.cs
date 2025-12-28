using CommunityToolkit.Mvvm.ComponentModel;

namespace BombaProMax.Models;

public partial class CreditTransactionDto : ObservableObject
{
    public int CreditID { get; set; }
    public string? NumeroTransaction { get; set; }
    public int ClientID { get; set; }
    public int? ProduitID { get; set; }
    public int? ServiceID { get; set; }
    public decimal PrixTTC { get; set; }
    public int Quantite { get; set; }
    public decimal MontantTotal { get; set; }
    public DateTime DateCredit { get; set; }
    public bool Facture { get; set; }
    public int? FactureID { get; set; }
    
    // BL link fields
    public int? BonLivraisonID { get; set; }
    public bool EstEnBL { get; set; }
    
    // Periode link field (for carburant credits during a shift)
    public int? PeriodeID { get; set; }
    
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Display fields for related entities
    public string? ClientNom { get; set; }
    public string? ProduitNom { get; set; }
    public string? ServiceNom { get; set; }
    public string? FactureNumero { get; set; }
    public string? BonLivraisonNumero { get; set; }

    // UI selection (observable for binding)
    [ObservableProperty]
    private bool _isSelected;
    
    // Computed display property
    public string ItemDescription => ProduitNom ?? ServiceNom ?? "N/A";
}

/// <summary>
/// Request DTO for converting CreditTransactions to a BonLivraison
/// </summary>
public class CreateBLFromCTsDto
{
    public List<int> CreditTransactionIds { get; set; } = [];
    public int ClientID { get; set; }
    public DateOnly? DateBL { get; set; }
    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
}

/// <summary>
/// Request DTO for converting CreditTransactions directly to a Facture
/// </summary>
public class CreateFactureFromCTsDto
{
    public List<int> CreditTransactionIds { get; set; } = [];
    public int ClientID { get; set; }
    public DateOnly? DateFacture { get; set; }
    public int? MoyenPaiementID { get; set; }
    public int? CreatedByUserId { get; set; }
}

/// <summary>
/// Result DTO for CT conversion operations
/// </summary>
public class CTConversionResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    
    // For BL creation
    public int? BonLivraisonID { get; set; }
    public string? NumeroBL { get; set; }
    
    // For direct Facture creation
    public int? FactureID { get; set; }
    public string? NumeroFacture { get; set; }
    
    public decimal MontantTotal { get; set; }
    public int CTsConverted { get; set; }
    public List<string>? Errors { get; set; }
}
