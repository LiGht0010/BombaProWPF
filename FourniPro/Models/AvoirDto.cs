namespace FourniPro.Models;

public class AvoirDto
{
    public int AvoirId { get; set; }
    public string? NumeroAvoir { get; set; }
    public DateOnly DateAvoir { get; set; }

    // ── Source ────────────────────────────────────────────────────────────────

    public int? VenteId { get; set; }
    public string? NumeroVente { get; set; }

    public int? CreditId { get; set; }
    public string? NumeroCredit { get; set; }

    // ── FKs + resolved names ──────────────────────────────────────────────────

    public int? ProduitId { get; set; }
    public string? ProduitNom { get; set; }

    public int? ClientId { get; set; }
    public string? ClientNom { get; set; }

    public int? EmployeId { get; set; }
    public string? EmployeNom { get; set; }

    // ── Financial ─────────────────────────────────────────────────────────────

    public int? Quantite { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? MontantAvoir { get; set; }

    // ── Classification ────────────────────────────────────────────────────────

    public string? Raison { get; set; }

    // ── Metadata ──────────────────────────────────────────────────────────────

    public string? Note { get; set; }
    public string? ReferenceFile { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }

    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
