namespace BombaProMax.Models;

/// <summary>
/// Data model for Periode PDF generation.
/// Contains all the data needed to generate a complete PDF report.
/// </summary>
public class PeriodePdfData
{
    public int PeriodeID { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string? EmployeNom { get; set; }

    // Financial data
    public decimal TPE { get; set; }
    public decimal Especes { get; set; }
    public decimal Recette { get; set; }
    public decimal TotalQuantite { get; set; }
    public decimal TotalCoutAchat { get; set; }

    // Grouped data
    public List<ReservoirPdfData> TotauxParReservoir { get; set; } = [];
    public List<ProduitPdfData> TotauxParProduit { get; set; } = [];
    public List<MargePdfData> MargesParProduit { get; set; } = [];
    public List<PompePdfData> DetailsParPompe { get; set; } = [];
}

public class ReservoirPdfData
{
    public string ReservoirNumero { get; set; } = "";
    public string? ProduitNom { get; set; }
    public decimal QuantiteConsommee { get; set; }
    public decimal Montant { get; set; }
}

public class ProduitPdfData
{
    public string ProduitNom { get; set; } = "";
    public decimal Quantite { get; set; }
    public decimal Montant { get; set; }
}

public class MargePdfData
{
    public string? ProduitNom { get; set; }
    public decimal Quantite { get; set; }
    public decimal CoutAchat { get; set; }
    public decimal Vente { get; set; }
    public decimal Marge => Vente - CoutAchat;
    public decimal MargePercent => Vente > 0 ? Math.Round((Marge / Vente) * 100, 1) : 0;
}

public class PompePdfData
{
    public string PompeNumero { get; set; } = "";
    public string? ProduitNom { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal CompteurElecDebut { get; set; }
    public decimal CompteurElecFin { get; set; }
    public decimal CompteurMecaDebut { get; set; }
    public decimal CompteurMecaFin { get; set; }
    public decimal QuantiteVendue { get; set; }
    public decimal Montant { get; set; }
}
