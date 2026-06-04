namespace FourniProApi.DTOs;

/// <summary>
/// Used for both GET responses and POST/PUT request bodies.
/// </summary>
public class ClientDto
{
    public int ClientId { get; set; }

    public string NumeroClient { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string? Contact { get; set; }
    public string CIN { get; set; } = null!;
    public string NomSociete { get; set; } = null!;
    public string? Adresse { get; set; }
    public string? Personel { get; set; }

    // Audit — IDs
    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
