namespace FourniPro.Models;

/// <summary>
/// Mirrors FourniProApi.DTOs.ProduitDto — deserialized from API responses.
/// </summary>
public class ProduitDto
{
    public int ProduitId { get; set; }
    public string NumeroProduit { get; set; } = null!;
    public string? Description { get; set; }

    public decimal? PrixAchat { get; set; }
    public decimal? PrixHT { get; set; }
    public decimal? TVA { get; set; }
    public decimal? PrixTTC { get; set; }

    // Read-only — computed by the API
    public decimal? MargeBeneficiaire { get; set; }
    public decimal? MargePourcentage { get; set; }

    public int? Stock { get; set; }
    public int? StockMinimum { get; set; }
    public int? DelaiDeLivraison { get; set; }

    // Audit
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }

    /// <summary>True when stock is at or below the minimum threshold.</summary>
    public bool IsLowStock =>
        Stock.HasValue && StockMinimum.HasValue && Stock.Value <= StockMinimum.Value;

    /// <summary>Display label used in lists and detail views.</summary>
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Description) ? Description : NumeroProduit;
}
