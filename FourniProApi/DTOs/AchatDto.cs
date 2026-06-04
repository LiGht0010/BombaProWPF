namespace FourniProApi.DTOs;

/// <summary>
/// Used for both GET responses and POST/PUT request bodies.
/// </summary>
public class AchatDto
{
    public int AchatId { get; set; }
    public string? Numero { get; set; }
    public DateOnly Date { get; set; }

    public int? FournisseurID { get; set; }
    public string? FournisseurNom { get; set; }

    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }

    public int? Quantite { get; set; }
    public decimal? Cout { get; set; }
    public decimal? PrixAchatUnitaire { get; set; }

    public bool? LivraisonDefectueuse { get; set; }
    public string? Description { get; set; }

    // Audit
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
