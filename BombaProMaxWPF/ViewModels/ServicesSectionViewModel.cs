using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// Section-scoped view-model for the Services sub-section of the
/// Infrastructure shell. Lists all services and exposes add / edit / delete
/// actions via self-contained dialogs.
/// </summary>
public partial class ServicesSectionViewModel : ObservableObject, IAsyncLoadable
{
    private readonly ServiceService _serviceService = new();

    public ObservableCollection<ServiceCardItem> Services { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string _searchText = string.Empty;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddServiceCommand { get; }
    public IRelayCommand ManageCategoriesCommand { get; }
    public IRelayCommand<ServiceCardItem> EditServiceCommand { get; }
    public IRelayCommand<ServiceCardItem> DetailServiceCommand { get; }
    public IAsyncRelayCommand<ServiceCardItem> DeleteServiceCommand { get; }

    public ServicesSectionViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(ct => RefreshAsync(ct));
        AddServiceCommand = new RelayCommand(OpenAddService);
        ManageCategoriesCommand = new RelayCommand(OpenManageCategories);
        EditServiceCommand = new RelayCommand<ServiceCardItem>(OpenEditService);
        DetailServiceCommand = new RelayCommand<ServiceCardItem>(OpenDetailService);
        DeleteServiceCommand = new AsyncRelayCommand<ServiceCardItem>(DeleteServiceAsync);
    }

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (IsLoaded) return;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        IsLoaded = false;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var list = await _serviceService.GetAllServicesAsync().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Services.Clear();
                foreach (var s in list)
                    Services.Add(new ServiceCardItem(s));
                IsLoaded = true;
            });
        }
        catch (OperationCanceledException) { /* navigated away */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[ServicesSectionVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddService()
    {
        try
        {
            ErrorMessage = null;
            var dialog = new Views.InfrastructurePages.Sections.Services.NouveauServiceDialog
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            if (dialog.ViewModel.Saved)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ServicesSectionVM] OpenAddService failed: {ex}");
        }
    }

    private void OpenManageCategories()
    {
        try
        {
            ErrorMessage = null;
            var dialog = new Views.InfrastructurePages.Sections.Services.GererServiceCategoriesDialog
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ServicesSectionVM] OpenManageCategories failed: {ex}");
        }
    }

    private void OpenEditService(ServiceCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dialog = new Views.InfrastructurePages.Sections.Services.EditServiceDialog(item.Dto)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            if (dialog.ViewModel.Saved)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ServicesSectionVM] OpenEditService failed: {ex}");
        }
    }

    private void OpenDetailService(ServiceCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dialog = new Views.InfrastructurePages.Sections.Services.DetailServiceDialog(item.Dto)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            if (dialog.ShouldEdit)
                OpenEditService(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ServicesSectionVM] OpenDetailService failed: {ex}");
        }
    }

    private async Task DeleteServiceAsync(ServiceCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            var ok = await _serviceService.DeleteServiceAsync(item.ID).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                {
                    Services.Remove(item);
                    SuccessMessage = LanguageManager.Instance["SrvDeleteSuccess"];
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["SrvDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[ServicesSectionVM] Delete failed: {ex}");
        }
    }
}

/// <summary>
/// Observable wrapper around a <see cref="ServiceDto"/> for use in the
/// Services section list.
/// </summary>
public class ServiceCardItem
{
    private readonly ServiceDto _dto;

    public ServiceCardItem(ServiceDto dto) => _dto = dto;

    public int ID => _dto.ID;
    public ServiceDto Dto => _dto;
    public string Numero => string.IsNullOrWhiteSpace(_dto.Numero) ? "—" : _dto.Numero!;
    public string Description => string.IsNullOrWhiteSpace(_dto.Description) ? "—" : _dto.Description!;
    public string Categorie => string.IsNullOrWhiteSpace(_dto.ServiceCategorieNom) ? "—" : _dto.ServiceCategorieNom!;
    public string PrixDisplay => _dto.Prix.HasValue ? $"{_dto.Prix:N2} DH" : "—";
}
