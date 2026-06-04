using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Clients;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped view-model for the Clients sub-section of the
/// Infrastructure shell. Lists all clients and exposes add / edit / detail / delete actions.
/// </summary>
public partial class ClientsSectionViewModel : ObservableObject
{
    private readonly ClientService _service = new();
    private bool _isLoaded;

    public ObservableCollection<ClientCardItem> Clients { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddClientCommand { get; }
    public IAsyncRelayCommand<ClientCardItem> EditClientCommand { get; }
    public IAsyncRelayCommand<ClientCardItem> DetailClientCommand { get; }
    public IAsyncRelayCommand<ClientCardItem> DeleteClientCommand { get; }

    public ClientsSectionViewModel()
    {
        RefreshCommand      = new AsyncRelayCommand(ct => RefreshAsync(ct));
        AddClientCommand    = new RelayCommand(OpenAddClient);
        EditClientCommand   = new AsyncRelayCommand<ClientCardItem>(OpenEditClientAsync);
        DetailClientCommand = new AsyncRelayCommand<ClientCardItem>(OpenDetailClientAsync);
        DeleteClientCommand = new AsyncRelayCommand<ClientCardItem>(DeleteClientAsync);
    }

    /// <summary>Loads clients once; subsequent calls are no-ops unless RefreshAsync is used.</summary>
    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_isLoaded) return;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        _isLoaded = false;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = null;

            var list = await _service.GetAllClientsAsync().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Clients.Clear();
                foreach (var c in list)
                    Clients.Add(new ClientCardItem(c));
                _isLoaded = true;
            });
        }
        catch (OperationCanceledException) { /* navigated away — no-op */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[ClientsSectionVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddClient()
    {
        try
        {
            ErrorMessage = null;
            var dialog = new NouveauClientDialog
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
            Debug.WriteLine($"[ClientsSectionVM] OpenAddClient failed: {ex}");
        }
    }

    private async Task OpenEditClientAsync(ClientCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dto = await _service.GetClientByIdAsync(item.ClientId).ConfigureAwait(false);
            if (dto is null)
            {
                ErrorMessage = LanguageManager.Instance["ClientNotFound"];
                return;
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new EditClientDialog(dto)
                {
                    Owner = Application.Current?.MainWindow
                };
                dialog.ShowDialog();
                if (dialog.ViewModel.Saved)
                    _ = RefreshAsync(CancellationToken.None);
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ClientsSectionVM] OpenEditClient failed: {ex}");
        }
    }

    private async Task OpenDetailClientAsync(ClientCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dto = await _service.GetClientByIdAsync(item.ClientId).ConfigureAwait(false);
            if (dto is null) { ErrorMessage = LanguageManager.Instance["ClientNotFound"]; return; }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var detail = new DetailClientDialog(dto)
                {
                    Owner = Application.Current?.MainWindow
                };
                detail.ShowDialog();

                if (detail.ShouldEdit)
                {
                    var edit = new EditClientDialog(dto)
                    {
                        Owner = Application.Current?.MainWindow
                    };
                    edit.ShowDialog();
                    if (edit.ViewModel.Saved)
                        _ = RefreshAsync(CancellationToken.None);
                }
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ClientsSectionVM] OpenDetailClient failed: {ex}");
        }
    }

    private async Task DeleteClientAsync(ClientCardItem? item)
    {
        if (item is null) return;

        var confirm = MessageBox.Show(
            $"Supprimer « {item.Nom} » ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            ErrorMessage   = null;
            SuccessMessage = null;

            var ok = await _service.DeleteClientAsync(item.ClientId).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                {
                    Clients.Remove(item);
                    SuccessMessage = LanguageManager.Instance["ClientDeleteSuccess"];
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["ClientDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[ClientsSectionVM] Delete failed: {ex}");
        }
    }
}
