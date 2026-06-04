using System;
using System.ComponentModel.DataAnnotations;

namespace FourniProApi.Models;

public class Camion
{
    [Key]
    public int CamionId { get; set; }

    [StringLength(20)]
    public string? Matricule { get; set; }

    [StringLength(50)]
    public string? Marque { get; set; }

    public double? Consommation { get; set; }
    public double? Kilometrage { get; set; }

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
