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

    // Computed property for duration
    public TimeSpan Duree => DateFin - DateDebut;
    public string DureeFormatted => $"{(int)Duree.TotalHours}h {Duree.Minutes:D2}min";

    // Financial data
    public decimal TPE { get; set; }
    public decimal Especes { get; set; }
    public decimal Recette { get; set; }
    public decimal TotalQuantite { get; set; }
    public decimal TotalCoutAchat { get; set; }
    
    // Credit transactions total
    public decimal TotalCredite { get; set; }
    public decimal EspecesAttendues => Recette - TPE - TotalCredite;
    public decimal Manque => EspecesAttendues - Especes;

    // Grouped data
    public List<ReservoirPdfData> TotauxParReservoir { get; set; } = [];
    public List<ProduitPdfData> TotauxParProduit { get; set; } = [];
    public List<MargePdfData> MargesParProduit { get; set; } = [];
    public List<PompePdfData> DetailsParPompe { get; set; } = [];
    
    /// <summary>
    /// Individual stock lot consumptions for detailed margin analysis
    /// </summary>
    public List<StockLotConsumptionPdfData> ConsommationsStock { get; set; } = [];
    
    /// <summary>
    /// Credit transactions linked to this periode
    /// </summary>
    public List<CreditTransactionPdfData> CreditTransactions { get; set; } = [];
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

/// <summary>
/// Individual stock lot consumption for detailed margin PDF
/// </summary>
public class StockLotConsumptionPdfData
{
    public int StockLotID { get; set; }
    public string? ProduitNom { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal QuantiteConsommee { get; set; }
    public decimal PrixAchat { get; set; }
    public decimal PrixVente { get; set; }
    
    public decimal CoutAchat => QuantiteConsommee * PrixAchat;
    public decimal Vente => QuantiteConsommee * PrixVente;
    public decimal Marge => Vente - CoutAchat;
    public decimal MargePercent => Vente > 0 ? Math.Round((Marge / Vente) * 100, 1) : 0;
}

/// <summary>
/// Credit transaction data for PDF
/// </summary>
public class CreditTransactionPdfData
{
    public int CreditID { get; set; }
    public string? ClientNom { get; set; }
    public string? ProduitNom { get; set; }
    public DateTime DateCredit { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal MontantTotal { get; set; }
}
