using System;
using System.ComponentModel.DataAnnotations;

namespace FourniProApi.Models;

public class Chauffeur
{
    [Key]
    public int ChauffeurId { get; set; }

    [Required, StringLength(50)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Prenom { get; set; }

    [StringLength(20)]
    public string? CIN { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(50)]
    public string? NumeroPermis { get; set; }

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
