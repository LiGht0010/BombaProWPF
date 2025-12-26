using System.ComponentModel.DataAnnotations;

namespace BombaProMaxApi.Models;

public class DepenseCategorie
{
    [Key]
    public int ID { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
    [StringLength(100)]
    public string Nom { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    [StringLength(50)]
    public string? CreePar { get; set; }

    public DateTime? DateCreation { get; set; }

    [StringLength(50)]
    public string? ModifiePar { get; set; }

    public DateTime? DateModification { get; set; }
}
