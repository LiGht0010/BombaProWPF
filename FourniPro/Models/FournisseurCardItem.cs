namespace FourniPro.Models;

/// <summary>
/// Wraps a <see cref="FournisseurDto"/> and exposes display-friendly computed
/// properties for the fournisseurs table view.
/// </summary>
public class FournisseurCardItem(FournisseurDto dto)
{
    public FournisseurDto Dto => dto;
    public int FournisseurId => dto.FournisseurId;

    public string NomComplet =>
        $"{dto.Prenom} {dto.Nom}".Trim() is { Length: > 0 } n ? n : "—";

    public string Societe =>
        string.IsNullOrWhiteSpace(dto.Societe) ? "—" : dto.Societe;

    public string Contact =>
        !string.IsNullOrWhiteSpace(dto.Contact) ? dto.Contact :
        $"{dto.Prenom} {dto.Nom}".Trim() is { Length: > 0 } n ? n : "—";

    public string Telephone =>
        string.IsNullOrWhiteSpace(dto.Telephone) ? "—" : dto.Telephone;

    public string Email =>
        string.IsNullOrWhiteSpace(dto.Email) ? "—" : dto.Email;

    public string Statut => dto.Statut;

    /// <summary>"Actif" | "Inactif" — drives badge colour.</summary>
    public bool IsActif =>
        string.Equals(dto.Statut, "Actif", StringComparison.OrdinalIgnoreCase);
}
