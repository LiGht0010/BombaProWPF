using System.ComponentModel.DataAnnotations;

namespace FourniProApi.Models;

public class Client
{
    [Key]
    public int ClientId { get; set; }

    [Required]
    [StringLength(20)]
    public string NumeroClient { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Nom { get; set; } = null!;

    [StringLength(100)]
    public string? Contact { get; set; }

    [Required]
    [StringLength(100)]
    public string CIN { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string NomSociete { get; set; } = null!;

    [StringLength(200)]
    public string? Adresse { get; set; }

    [StringLength(100)]
    public string? Personel { get; set; }

    // Audit fields
    public int? AjoutePar { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
