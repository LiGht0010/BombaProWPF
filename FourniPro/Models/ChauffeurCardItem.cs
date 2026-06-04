namespace FourniPro.Models;

public class ChauffeurCardItem
{
    public int ChauffeurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string NomComplet => string.IsNullOrWhiteSpace(Prenom) ? Nom : $"{Prenom} {Nom}";
    public string? CIN { get; set; }
    public string? Telephone { get; set; }
    public string? NumeroPermis { get; set; }
}
