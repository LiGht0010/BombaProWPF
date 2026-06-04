using System;

namespace FourniPro.Models;

public class CamionDto
{
    public int CamionId { get; set; }
    public string? Matricule { get; set; }
    public string? Marque { get; set; }
    public double? Consommation { get; set; }
    public double? Kilometrage { get; set; }

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
