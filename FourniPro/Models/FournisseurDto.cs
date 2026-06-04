namespace FourniPro.Models;

/// <summary>
/// Mirrors FourniProApi.DTOs.FournisseurDto — deserialized from API responses.
/// </summary>
public class FournisseurDto
{
    public int FournisseurId { get; set; }

    public string? Prenom { get; set; }
    public string? Nom { get; set; }
    public string? Societe { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? RIB { get; set; }
    public string? Contact { get; set; }
    public string? ConditionsPaiement { get; set; }
    public string Statut { get; set; } = "Actif";

    // Audit
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }

    /// <summary>Display name: Société if set, otherwise Prénom + Nom.</summary>
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Societe)
            ? Societe
            : $"{Prenom} {Nom}".Trim();
}
