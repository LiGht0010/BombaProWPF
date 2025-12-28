using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class ServiceViewModel : ObservableObject
{
    private readonly ServiceService _serviceService;
    private readonly ServiceCategorieService _serviceCategorieService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<ServiceDto> Services { get; } = new();
    public ObservableCollection<ServiceCategorieDto> Categories { get; } = new();
    public ObservableCollection<ServiceCategorieDto> CategoriesList { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCategoriesLoading;

    [ObservableProperty]
    private ServiceDto? _selectedService;

    [ObservableProperty]
    private ServiceCategorieDto? _selectedCategory;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showInactiveCategories;

    // Stats
    [ObservableProperty]
    private int _totalServices;

    [ObservableProperty]
    private int _totalCategories;

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private decimal _averagePrice;

    public ServiceViewModel(
        ServiceService serviceService,
        ServiceCategorieService serviceCategorieService,
        IDialogService dialogService)
    {
        _serviceService = serviceService;
        _serviceCategorieService = serviceCategorieService;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Called when ShowInactiveCategories property changes - auto refresh the list
    /// </summary>
    partial void OnShowInactiveCategoriesChanged(bool value)
    {
        _ = LoadCategoriesListAsync();
    }

    // ????????????????????????????????????????????????????????????????
    // INITIALIZATION
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadServicesAsync();
        await LoadCategoriesListAsync();
    }

    // ????????????????????????????????????????????????????????????????
    // LOAD COMMANDS
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task LoadServicesAsync()
    {
        try
        {
            IsLoading = true;
            var services = await _serviceService.GetAllServicesAsync();
            Services.Clear();
            foreach (var service in services)
            {
                Services.Add(service);
            }
            UpdateStats();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les services: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _serviceCategorieService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories.Where(c => c.IsActive))
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les categories: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task LoadCategoriesListAsync()
    {
        try
        {
            IsCategoriesLoading = true;
            var categories = await _serviceCategorieService.GetAllCategoriesAsync();
            CategoriesList.Clear();
            
            var filtered = ShowInactiveCategories 
                ? categories 
                : categories.Where(c => c.IsActive);
                
            foreach (var category in filtered)
            {
                CategoriesList.Add(category);
            }
            TotalCategories = CategoriesList.Count;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les categories: {ex.Message}");
        }
        finally
        {
            IsCategoriesLoading = false;
        }
    }

    private void UpdateStats()
    {
        TotalServices = Services.Count;
        TotalRevenue = Services.Sum(s => s.Prix ?? 0);
        AveragePrice = Services.Count > 0 ? TotalRevenue / Services.Count : 0;
    }

    // ????????????????????????????????????????????????????????????????
    // SERVICE CRUD - Called from code-behind with popup results
    // ????????????????????????????????????????????????????????????????

    public async Task CreateServiceAsync(ServiceDto newService)
    {
        try
        {
            IsLoading = true;
            var created = await _serviceService.CreateServiceAsync(newService);
            if (created != null)
            {
                Services.Insert(0, created);
                UpdateStats();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de creer le service");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateServiceAsync(ServiceDto updatedService)
    {
        try
        {
            IsLoading = true;
            var success = await _serviceService.UpdateServiceAsync(updatedService);
            if (success)
            {
                await LoadServicesAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de modifier le service");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteServiceAsync(ServiceDto service)
    {
        try
        {
            IsLoading = true;
            var success = await _serviceService.DeleteServiceAsync(service.ID);
            if (success)
            {
                Services.Remove(service);
                UpdateStats();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Echec de la suppression");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ????????????????????????????????????????????????????????????????
    // CATEGORY CRUD - Called from code-behind with popup results
    // ????????????????????????????????????????????????????????????????

    public async Task CreateCategorieAsync(ServiceCategorieDto newCategorie)
    {
        try
        {
            IsCategoriesLoading = true;
            var created = await _serviceCategorieService.CreateCategorieAsync(newCategorie);
            if (created != null)
            {
                CategoriesList.Insert(0, created);
                if (created.IsActive)
                {
                    Categories.Insert(0, created);
                }
                TotalCategories = CategoriesList.Count;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de creer la categorie. Le nom existe peut-etre deja.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsCategoriesLoading = false;
        }
    }

    public async Task UpdateCategorieAsync(ServiceCategorieDto updatedCategorie)
    {
        try
        {
            IsCategoriesLoading = true;
            var success = await _serviceCategorieService.UpdateCategorieAsync(updatedCategorie);
            if (success)
            {
                await LoadCategoriesAsync();
                await LoadCategoriesListAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de modifier la categorie");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsCategoriesLoading = false;
        }
    }

    public async Task DeleteCategorieAsync(ServiceCategorieDto categorie)
    {
        try
        {
            IsCategoriesLoading = true;
            var success = await _serviceCategorieService.DeleteCategorieAsync(categorie.ID);
            if (success)
            {
                CategoriesList.Remove(categorie);
                var catInList = Categories.FirstOrDefault(c => c.ID == categorie.ID);
                if (catInList != null)
                {
                    Categories.Remove(catInList);
                }
                TotalCategories = CategoriesList.Count;
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Echec de la suppression");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsCategoriesLoading = false;
        }
    }

    // ????????????????????????????????????????????????????????????????
    // FILTER COMMANDS
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    private async Task FilterByCategoryAsync()
    {
        try
        {
            IsLoading = true;

            List<ServiceDto> services;
            if (SelectedCategory == null)
            {
                services = await _serviceService.GetAllServicesAsync();
            }
            else
            {
                services = await _serviceService.GetServicesByCategoryAsync(SelectedCategory.ID);
            }

            Services.Clear();
            foreach (var service in services)
            {
                Services.Add(service);
            }
            UpdateStats();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors du filtrage: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchServicesAsync()
    {
        try
        {
            IsLoading = true;
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadServicesAsync();
                return;
            }
            
            var results = await _serviceService.SearchServicesAsync(SearchText);
            Services.Clear();
            foreach (var service in results)
            {
                Services.Add(service);
            }
            UpdateStats();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la recherche: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
