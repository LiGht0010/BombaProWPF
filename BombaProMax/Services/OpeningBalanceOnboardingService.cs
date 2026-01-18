using BombaProMax.Models;

namespace BombaProMax.Services;

/// <summary>
/// Service to manage Opening Balance onboarding workflow.
/// Detects reservoirs needing initial stock setup and guides users through the process.
/// </summary>
public class OpeningBalanceOnboardingService
{
    private readonly StockLotService _stockLotService;
    private readonly ReservoirService _reservoirService;
    private readonly ProduitService _produitService;
    
    // Cache of reservoirs needing opening balance
    private List<ReservoirStockStatusDto>? _pendingReservoirs;
    private int _currentIndex;
    
    /// <summary>
    /// Event fired when onboarding is complete (all reservoirs processed or skipped).
    /// </summary>
    public event EventHandler? OnboardingCompleted;
    
    /// <summary>
    /// Event fired when a reservoir's opening balance is successfully created.
    /// </summary>
    public event EventHandler<OpeningBalanceResultDto>? OpeningBalanceCreated;

    public OpeningBalanceOnboardingService(
        StockLotService stockLotService,
        ReservoirService reservoirService,
        ProduitService produitService)
    {
        _stockLotService = stockLotService;
        _reservoirService = reservoirService;
        _produitService = produitService;
    }

    /// <summary>
    /// Checks if onboarding is needed (any reservoirs without stock lots).
    /// </summary>
    public async Task<bool> IsOnboardingNeededAsync()
    {
        try
        {
            var reservoirs = await _stockLotService.GetReservoirsNeedingOpeningBalanceAsync();
            _pendingReservoirs = reservoirs;
            _currentIndex = 0;
            
            System.Diagnostics.Debug.WriteLine(
                $"[OpeningBalanceOnboarding] Found {reservoirs.Count} reservoir(s) needing opening balance");
            
            return reservoirs.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[OpeningBalanceOnboarding] Error checking onboarding need: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the list of reservoirs pending opening balance setup.
    /// </summary>
    public List<ReservoirStockStatusDto> GetPendingReservoirs()
    {
        return _pendingReservoirs ?? new List<ReservoirStockStatusDto>();
    }

    /// <summary>
    /// Gets the current reservoir to process.
    /// </summary>
    public ReservoirStockStatusDto? GetCurrentReservoir()
    {
        if (_pendingReservoirs == null || _currentIndex >= _pendingReservoirs.Count)
            return null;
        
        return _pendingReservoirs[_currentIndex];
    }

    /// <summary>
    /// Gets the count of remaining reservoirs to process.
    /// </summary>
    public int RemainingCount => _pendingReservoirs != null 
        ? Math.Max(0, _pendingReservoirs.Count - _currentIndex) 
        : 0;

    /// <summary>
    /// Gets the total count of reservoirs needing opening balance.
    /// </summary>
    public int TotalCount => _pendingReservoirs?.Count ?? 0;

    /// <summary>
    /// Gets the current progress (1-based index).
    /// </summary>
    public int CurrentProgress => _currentIndex + 1;

    /// <summary>
    /// Checks if there are more reservoirs to process.
    /// </summary>
    public bool HasMoreReservoirs => _pendingReservoirs != null && _currentIndex < _pendingReservoirs.Count;

    /// <summary>
    /// Creates an opening balance for the current reservoir.
    /// </summary>
    public async Task<(bool Success, string Message)> CreateOpeningBalanceAsync(
        decimal quantite,
        decimal prixAchat = 0,
        string? notes = null)
    {
        var current = GetCurrentReservoir();
        if (current == null)
        {
            return (false, "Aucun réservoir à traiter");
        }

        if (!current.ProduitID.HasValue)
        {
            return (false, "Le réservoir n'a pas de produit assigné. Veuillez d'abord assigner un produit.");
        }

        var dto = new OpeningBalanceCreateDto
        {
            ReservoirID = current.ReservoirID,
            ProduitID = current.ProduitID.Value,
            Quantite = quantite,
            PrixAchat = prixAchat,
            Notes = notes ?? "Stock initial créé lors de l'installation"
        };

        var (success, result, errorMessage) = await _stockLotService.CreateOpeningBalanceAsync(dto);

        if (success && result != null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[OpeningBalanceOnboarding] Created opening balance for Reservoir {current.ReservoirNumero}: {quantite}L");
            
            OpeningBalanceCreated?.Invoke(this, result);
            MoveToNextReservoir();
            
            return (true, result.Message);
        }

        return (false, errorMessage ?? "Erreur lors de la création du stock initial");
    }

    /// <summary>
    /// Skips the current reservoir (user doesn't want to set opening balance now).
    /// </summary>
    public void SkipCurrentReservoir()
    {
        var current = GetCurrentReservoir();
        if (current != null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[OpeningBalanceOnboarding] Skipped Reservoir {current.ReservoirNumero}");
        }
        
        MoveToNextReservoir();
    }

    /// <summary>
    /// Skips all remaining reservoirs and completes onboarding.
    /// </summary>
    public void SkipAll()
    {
        System.Diagnostics.Debug.WriteLine(
            $"[OpeningBalanceOnboarding] Skipped all {RemainingCount} remaining reservoir(s)");
        
        _currentIndex = _pendingReservoirs?.Count ?? 0;
        CompleteOnboarding();
    }

    /// <summary>
    /// Moves to the next reservoir in the queue.
    /// </summary>
    private void MoveToNextReservoir()
    {
        _currentIndex++;
        
        if (!HasMoreReservoirs)
        {
            CompleteOnboarding();
        }
    }

    /// <summary>
    /// Completes the onboarding process.
    /// </summary>
    private void CompleteOnboarding()
    {
        System.Diagnostics.Debug.WriteLine("[OpeningBalanceOnboarding] Onboarding completed");
        OnboardingCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets the onboarding state (for testing or re-running).
    /// </summary>
    public void Reset()
    {
        _pendingReservoirs = null;
        _currentIndex = 0;
    }

    /// <summary>
    /// Gets available products for selection (carburant only).
    /// </summary>
    public async Task<List<ProduitDto>> GetCarburantProductsAsync()
    {
        try
        {
            var products = await _produitService.GetAllProduitsAsync();
            // Filter to CARBURANT category (CategorieID = 1 based on seeded data)
            return products
                .Where(p => string.Equals(p.CategorieNom, "CARBURANT", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OpeningBalanceOnboarding] Error getting products: {ex.Message}");
            return new List<ProduitDto>();
        }
    }
}
