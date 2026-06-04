namespace FourniProApi.DTOs;

/// <summary>
/// Used for both GET responses and POST/PUT request bodies.
/// <see cref="MargeBeneficiaire"/> and <see cref="MargePourcentage"/> are read-only computed fields.
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

    public int? Stock { get; set; }
    public int? StockMinimum { get; set; }
    public int? DelaiDeLivraison { get; set; }

    // Computed — populated by the mapping profile, ignored on write
    public decimal? MargeBeneficiaire { get; set; }
    public decimal? MargePourcentage { get; set; }

    // Audit
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
