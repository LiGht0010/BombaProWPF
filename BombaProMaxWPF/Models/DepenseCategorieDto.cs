namespace BombaProMaxWPF.Models;

public class DepenseCategorieDto
{
    public int ID { get; set; }
    public string Nom { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public string? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Display helper
    public override string ToString() => Nom;
}
