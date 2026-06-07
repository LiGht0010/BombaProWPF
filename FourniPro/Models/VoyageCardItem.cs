namespace FourniPro.Models;

public class VoyageCardItem
{
    public int VoyageId { get; set; }
    public string? CamionMatricule { get; set; }
    public string? ChauffeurNom { get; set; }
    public string? CiterneMatricule { get; set; }
    public DateTime? DateDepart { get; set; }
    public DateTime? DateFinal { get; set; }
    public string? LieuDepart { get; set; }
    public string? LieuTerminal { get; set; }
    public decimal? KilometrageDepart { get; set; }
    public decimal? KilometrageFinal { get; set; }
    public string Statut { get; set; } = "InProgress";

    /// <summary>Distance parcourue = KmFinal - KmDepart, null if either is missing.</summary>
    public decimal? DistanceParcourue =>
        KilometrageDepart.HasValue && KilometrageFinal.HasValue
            ? KilometrageFinal.Value - KilometrageDepart.Value
            : null;
}
