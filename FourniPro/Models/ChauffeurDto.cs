using System;

namespace FourniPro.Models;

public class ChauffeurDto
{
    public int ChauffeurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string? CIN { get; set; }
    public string? Telephone { get; set; }
    public string? NumeroPermis { get; set; }

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    public string? AjouteParNom { get; set; }
    public string? ModifieParNom { get; set; }
}
