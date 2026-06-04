using System.ComponentModel.DataAnnotations;

namespace FourniProApi.Models;

public class Citerne
{
    [Key]
    public int CiterneId { get; set; }

    [StringLength(50)]
    public string? MatriculeCiterne { get; set; }

    public decimal? Capacite { get; set; }

    public int? PartitionsNumber { get; set; }

    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
