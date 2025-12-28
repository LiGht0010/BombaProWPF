using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels;

/// <summary>
/// ViewModel for managing service sales.
/// </summary>
public partial class VenteServiceViewModel : ObservableObject
{
    private readonly VenteServiceService _venteService;
    private readonly ServiceService _serviceService;
    private readonly ServiceCategorieService _categorieService;
    private readonly MoyensPaiementService _moyensPaiementService;
    private readonly ClientService _clientService;
    private readonly JourneeNavigationService _journeeService;

    #region Observable Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private VenteServiceDto? _selectedVente;

    [ObservableProperty]
    private DateTime _filterStartDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _filterEndDate = DateTime.Today;

    [ObservableProperty]
    private ServiceCategorieDto? _selectedCategory;

    #endregion

    #region Collections

    public ObservableCollection<VenteServiceDto> Ventes { get; } = [];
    public ObservableCollection<ServiceDto> AvailableServices { get; } = [];
    public ObservableCollection<ServiceCategorieDto> Categories { get; } = [];
    public ObservableCollection<MoyensPaiementDto> MoyensPaiement { get; } = [];
    public ObservableCollection<ClientDto> Clients { get; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Total sales count.
    /// </summary>
    public int TotalVentes => Ventes.Count;

    /// <summary>
    /// Total quantity sold.
    /// </summary>
    public int TotalQuantite => Ventes.Sum(v => v.Quantite);

    /// <summary>
    /// Total revenue.
    /// </summary>
    public decimal TotalMontant => Ventes.Sum(v => v.MontantTotal);

    /// <summary>
    /// Average sale amount.
    /// </summary>
    public decimal MoyenneMontant => Ventes.Count > 0 ? TotalMontant / Ventes.Count : 0;

    #endregion

    // ????????????????????????????????????????????????????????????????
    // JOURNÉE PROPERTIES
    // ????????????????????????????????????????????????????????????????
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    #region Constructor

    public VenteServiceViewModel(JourneeNavigationService journeeService)
    {
        _venteService = new VenteServiceService();
        _serviceService = new ServiceService();
        _categorieService = new ServiceCategorieService();
        _moyensPaiementService = new MoyensPaiementService();
        _clientService = new ClientService();
        _journeeService = journeeService;

        _journeeService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(IsJourneeActive));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(JourneeStepInfo));
        };
    }

    #endregion

    // ????????????????????????????????????????????????????????????????
    // JOURNÉE COMMANDS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

    [RelayCommand]
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

    [RelayCommand]
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    // ????????????????????????????????????????????????????????????????
    // INITIALIZATION
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            LoadVentesAsync(),
            LoadServicesAsync(),
            LoadCategoriesAsync(),
            LoadMoyensPaiementAsync(),
            LoadClientsAsync()
        );
    }

    // ????????????????????????????????????????????????????????????????
    // LOAD OPERATIONS
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task LoadVentesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var ventes = await _venteService.GetAllAsync();

            Ventes.Clear();
            foreach (var vente in ventes)
            {
                Ventes.Add(vente);
            }

            NotifyTotalsChanged();
            Debug.WriteLine($"[VenteServiceVM] Loaded {Ventes.Count} ventes");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error loading ventes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadVentesByDateRangeAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var ventes = await _venteService.GetByDateRangeAsync(FilterStartDate, FilterEndDate);

            // Apply category filter if needed
            if (SelectedCategory != null)
            {
                ventes = ventes.Where(v =>
                    v.ServiceCategorieNom?.Equals(SelectedCategory.Nom, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();
            }

            Ventes.Clear();
            foreach (var vente in ventes)
            {
                Ventes.Add(vente);
            }

            NotifyTotalsChanged();
            Debug.WriteLine($"[VenteServiceVM] Loaded {Ventes.Count} ventes for date range");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error loading ventes by date: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchVentesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var ventes = await _venteService.SearchAsync(SearchText ?? "");

            Ventes.Clear();
            foreach (var vente in ventes)
            {
                Ventes.Add(vente);
            }

            NotifyTotalsChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de recherche: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error searching ventes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadServicesAsync()
    {
        try
        {
            var services = await _serviceService.GetAllServicesAsync();

            AvailableServices.Clear();
            foreach (var service in services)
            {
                AvailableServices.Add(service);
            }

            Debug.WriteLine($"[VenteServiceVM] Loaded {AvailableServices.Count} services");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteServiceVM] Error loading services: {ex.Message}");
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categorieService.GetAllCategoriesAsync();

            Categories.Clear();
            foreach (var category in categories.Where(c => c.IsActive))
            {
                Categories.Add(category);
            }

            Debug.WriteLine($"[VenteServiceVM] Loaded {Categories.Count} categories");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteServiceVM] Error loading categories: {ex.Message}");
        }
    }

    private async Task LoadMoyensPaiementAsync()
    {
        try
        {
            var moyens = await _moyensPaiementService.GetAllAsync();

            MoyensPaiement.Clear();
            foreach (var moyen in moyens)
            {
                MoyensPaiement.Add(moyen);
            }

            Debug.WriteLine($"[VenteServiceVM] Loaded {MoyensPaiement.Count} moyens de paiement");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteServiceVM] Error loading moyens de paiement: {ex.Message}");
        }
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            var clients = await _clientService.GetAllClientsAsync();

            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            Debug.WriteLine($"[VenteServiceVM] Loaded {Clients.Count} clients");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteServiceVM] Error loading clients: {ex.Message}");
        }
    }

    // ????????????????????????????????????????????????????????????????
    // CRUD OPERATIONS
    // ????????????????????????????????????????????????????????????????

    public async Task<VenteServiceDto?> CreateVenteAsync(VenteServiceDto vente)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var created = await _venteService.CreateAsync(vente);
            if (created != null)
            {
                Ventes.Insert(0, created);
                NotifyTotalsChanged();
                Debug.WriteLine($"[VenteServiceVM] Created vente {created.ID}");
            }

            return created;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de création: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error creating vente: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> UpdateVenteAsync(VenteServiceDto vente)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _venteService.UpdateAsync(vente);
            if (success)
            {
                // Update in collection
                var index = Ventes.ToList().FindIndex(v => v.ID == vente.ID);
                if (index >= 0)
                {
                    Ventes[index] = vente;
                }
                NotifyTotalsChanged();
                Debug.WriteLine($"[VenteServiceVM] Updated vente {vente.ID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error updating vente: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> DeleteVenteAsync(VenteServiceDto vente)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _venteService.DeleteAsync(vente.ID);
            if (success)
            {
                Ventes.Remove(vente);
                if (SelectedVente?.ID == vente.ID)
                {
                    SelectedVente = null;
                }
                NotifyTotalsChanged();
                Debug.WriteLine($"[VenteServiceVM] Deleted vente {vente.ID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[VenteServiceVM] Error deleting vente: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // ????????????????????????????????????????????????????????????????
    // HELPER METHODS
    // ????????????????????????????????????????????????????????????????

    private void NotifyTotalsChanged()
    {
        OnPropertyChanged(nameof(TotalVentes));
        OnPropertyChanged(nameof(TotalQuantite));
        OnPropertyChanged(nameof(TotalMontant));
        OnPropertyChanged(nameof(MoyenneMontant));
    }

    /// <summary>
    /// Clears any error message.
    /// </summary>
    public void ClearError()
    {
        ErrorMessage = null;
    }
}
