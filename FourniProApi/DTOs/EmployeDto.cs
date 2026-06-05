namespace FourniProApi.DTOs;

public class EmployeDto
{
    public int EmployeId { get; set; }

    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;

    public string? CIN { get; set; }
    public string? Telephone { get; set; }
    public string? Address { get; set; }
    public string? Poste { get; set; }
    public decimal? Salaire { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
