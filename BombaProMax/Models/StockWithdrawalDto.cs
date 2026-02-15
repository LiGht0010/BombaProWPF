namespace BombaProMax.Models;

/// <summary>
/// Request DTO for stock withdrawal from a reservoir.
/// Used by super admin to manually remove stock (FIFO order).
/// </summary>
public class StockWithdrawalRequestDto
{
    /// <summary>
    /// The reservoir to withdraw from
    /// </summary>
    public int ReservoirID { get; set; }

    /// <summary>
    /// Quantity to withdraw in liters
    /// </summary>
    public decimal Quantite { get; set; }

    /// <summary>
    /// Date of the withdrawal (user-selected)
    /// </summary>
    public DateTime? DateRetrait { get; set; }

    /// <summary>
    /// Reason/notes for the withdrawal (for audit trail)
    /// </summary>
    public string? Motif { get; set; }

    /// <summary>
    /// User performing the withdrawal
    /// </summary>
    public int? UtilisateurID { get; set; }

    /// <summary>
    /// User name for display
    /// </summary>
    public string? UtilisateurNom { get; set; }
}

/// <summary>
/// Response DTO for stock withdrawal result.
/// Contains details of which lots were affected.
/// </summary>
public class StockWithdrawalResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public decimal QuantiteRetiree { get; set; }
    public decimal NouveauNiveau { get; set; }
    public DateTime DateRetrait { get; set; }

    /// <summary>
    /// Details of lots affected by this withdrawal (FIFO breakdown)
    /// </summary>
    public List<StockLotWithdrawalDetailDto> LotsAffectes { get; set; } = [];
}

/// <summary>
/// Detail of a single lot affected by withdrawal
/// </summary>
public class StockLotWithdrawalDetailDto
{
    public int StockLotID { get; set; }
    public decimal QuantiteAvant { get; set; }
    public decimal QuantiteRetiree { get; set; }
    public decimal QuantiteApres { get; set; }
    public decimal PrixAchat { get; set; }
    public string? Statut { get; set; }
    public bool EstEpuise { get; set; }
}

/// <summary>
/// DTO for displaying withdrawal history
/// </summary>
public class StockWithdrawalHistoryDto
{
    public int ID { get; set; }
    public int ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    public decimal Quantite { get; set; }
    public string? Motif { get; set; }
    public int? UtilisateurID { get; set; }
    public string? UtilisateurNom { get; set; }
    public DateTime DateRetrait { get; set; }

    /// <summary>
    /// Formatted date for display
    /// </summary>
    public string DateRetraitFormatted => DateRetrait.ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Formatted quantity for display
    /// </summary>
    public string QuantiteFormatted => $"{Quantite:N2} L";
}

/// <summary>
/// DTO for reservoir selection in withdrawal UI
/// </summary>
public class ReservoirWithdrawalInfoDto
{
    public int ID { get; set; }
    public string Numero { get; set; } = null!;
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    public decimal Capacite { get; set; }
    public decimal NiveauActuel { get; set; }
    public decimal StockDisponible { get; set; }
    public int NombreLots { get; set; }

    /// <summary>
    /// Display text for picker
    /// </summary>
    public string DisplayText => $"{Numero} - {ProduitNom ?? "Vide"} ({StockDisponible:N0}L disponible)";

    /// <summary>
    /// Fill percentage for visual indicator
    /// </summary>
    public decimal PourcentageRemplissage => Capacite > 0
        ? Math.Round((NiveauActuel / Capacite) * 100, 1)
        : 0;

    /// <summary>
    /// Can withdraw from this reservoir
    /// </summary>
    public bool PeutRetirer => StockDisponible > 0;
}
