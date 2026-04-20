namespace BombaProMaxWPF.Models;

/// <summary>
/// Data model for Achat (Purchase) PDF generation.
/// Contains all the data needed to generate a complete Achat PDF report.
/// </summary>
public class AchatPdfData
{
    public int AchatID { get; set; }
    public string Numero { get; set; } = "";
    public DateOnly Date { get; set; }

    // Fournisseur info
    public string? FournisseurNom { get; set; }

    // Produit info
    public string? ProduitNom { get; set; }

    // Quantities and pricing
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal CoutTotal { get; set; }

    // Transport info
    public string? ChauffeurNom { get; set; }
    public string? CamionImmatriculation { get; set; }

    // Delivery status
    public bool LivraisonDefectueuse { get; set; }

    // Allocation status
    public bool EstCarburant { get; set; }
    public decimal TotalAlloue { get; set; }
    public decimal QuantiteRestante { get; set; }
    public bool EstCompletementAlloue { get; set; }

    // Allocation details (which reservoirs received the fuel)
    public List<AchatAllocationPdfData> Allocations { get; set; } = [];
}

/// <summary>
/// Allocation detail for Achat PDF - shows which reservoir received how much fuel.
/// </summary>
public class AchatAllocationPdfData
{
    public string ReservoirNumero { get; set; } = "";
    public string? ProduitNom { get; set; }
    public decimal QuantiteAllouee { get; set; }
    public DateTime DateAllocation { get; set; }
    public string Statut { get; set; } = "";
}
