using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Represents a category for services (e.g., Lavage, Vidange, etc.)
/// </summary>
public class ServiceCategorie
{
    [Key]
    public int ID { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Nom de Catégorie")]
    public string Nom { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    [Display(Name = "Créé Par")]
    public int? CreePar { get; set; }

    [Display(Name = "Date de Création")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }

    // Navigation property for related services
    [InverseProperty("ServiceCategorie")]
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
