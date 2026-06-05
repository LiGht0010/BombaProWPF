namespace FourniPro.Models;

public class VoyageDto
{
    public int VoyageId { get; set; }

    public int? CamionId { get; set; }
    public string? CamionMatricule { get; set; }

    public int? ChauffeurId { get; set; }
    public string? ChauffeurNom { get; set; }

    public int? CiterneId { get; set; }
    public string? CiterneMatricule { get; set; }

    public DateTime? DateDepart { get; set; }
    public DateTime? DateFinal { get; set; }

    public string? LieuDepart { get; set; }
    public string? LieuTerminal { get; set; }

    public decimal? KilometrageDepart { get; set; }
    public decimal? KilometrageFinal { get; set; }

    public string Statut { get; set; } = "InProgress";

    public int? AjoutePar { get; set; }
    public string? AjouteParNom { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public string? ModifieParNom { get; set; }
    public DateTime? DateModification { get; set; }
}
