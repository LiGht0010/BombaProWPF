namespace BombaProMaxWPF.Models;

/// <summary>
/// Data Transfer Object for ServiceCategorie
/// </summary>
public class ServiceCategorieDto
{
    public int ID { get; set; }
    public string Nom { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }
}
