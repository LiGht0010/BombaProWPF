using CommunityToolkit.Mvvm.ComponentModel;

namespace BombaProMax.Models;

public partial class FactureDto : ObservableObject
{
    public int ID { get; set; }
    public string? NumeroFacture { get; set; }
    public DateOnly? DateFacture { get; set; }
    public int? ClientID { get; set; }
    public decimal? MontantTotal { get; set; }
    public decimal? MontantHT { get; set; }
    public decimal? MontantTVA { get; set; }
    public string? Statut { get; set; }
    public int? MoyenPaiementID { get; set; }
    public DateOnly? DatePaiement { get; set; }

    // Display fields for related entities
    public string? ClientNom { get; set; }
    public string? ClientNumero { get; set; }
    public string? ClientAdresse { get; set; }
    public string? ClientContact { get; set; }
    public string? ClientICE { get; set; }
    public string? ClientIF { get; set; }
    public string? MoyenPaiementNom { get; set; }

    // UI selection (observable for binding)
    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>
/// Request DTO for merging multiple Factures into one
/// </summary>
public class MergeFacturesDto
{
    public List<int> FactureIds { get; set; } = [];
    public int ClientID { get; set; }
    public DateOnly? DateFacture { get; set; }
    public int? MoyenPaiementID { get; set; }
    public int? CreatedByUserId { get; set; }
}

/// <summary>
/// Result DTO for Facture merge operations
/// </summary>
public class MergeFacturesResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? NewFactureID { get; set; }
    public string? NewNumeroFacture { get; set; }
    public decimal MontantTotal { get; set; }
    public int FacturesMerged { get; set; }
    public List<string>? Errors { get; set; }
}
