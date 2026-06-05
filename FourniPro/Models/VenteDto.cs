namespace FourniPro.Models;

public class VenteDto
{
    public int VenteId { get; set; }
    public string? NumeroVente { get; set; }
    public DateOnly DateVente { get; set; }

    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }

    public int? ClientID { get; set; }
    public string? ClientNom { get; set; }

    public int? VoyageID { get; set; }
    public string? VoyageNumero { get; set; }
    public int? EmployeId { get; set; }
    public string? EmployeNom { get; set; }

    public int? Quantite { get; set; }
    public decimal? PrixUnitaire { get; set; }
    public decimal? Remise { get; set; }
    public decimal? MontantTotal { get; set; }

    public string? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceFile { get; set; }

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
