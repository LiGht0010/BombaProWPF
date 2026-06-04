using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Fournisseurs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped view-model for the Fournisseurs sub-section of the
/// Infrastructure shell. Lists all fournisseurs and exposes add / edit / delete
/// actions.
/// </summary>
public partial class FournisseursSectionViewModel : ObservableObject
{
    private readonly FournisseurService _service = new();
    private bool _isLoaded;

    public ObservableCollection<FournisseurCardItem> Fournisseurs { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddFournisseurCommand { get; }
    public IAsyncRelayCommand<FournisseurCardItem> EditFournisseurCommand { get; }
    public IAsyncRelayCommand<FournisseurCardItem> DetailFournisseurCommand { get; }
    public IAsyncRelayCommand<FournisseurCardItem> DeleteFournisseurCommand { get; }

    public FournisseursSectionViewModel()
    {
        RefreshCommand           = new AsyncRelayCommand(ct => RefreshAsync(ct));
        AddFournisseurCommand    = new RelayCommand(OpenAddFournisseur);
        EditFournisseurCommand   = new AsyncRelayCommand<FournisseurCardItem>(OpenEditFournisseurAsync);
        DetailFournisseurCommand = new AsyncRelayCommand<FournisseurCardItem>(OpenDetailFournisseurAsync);
        DeleteFournisseurCommand = new AsyncRelayCommand<FournisseurCardItem>(DeleteFournisseurAsync);
    }

    /// <summary>Loads fournisseurs once; subsequent calls are no-ops unless RefreshAsync is used.</summary>
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

            var list = await _service.GetAllFournisseursAsync().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Fournisseurs.Clear();
                foreach (var f in list)
                    Fournisseurs.Add(new FournisseurCardItem(f));
                _isLoaded = true;
            });
        }
        catch (OperationCanceledException) { /* navigated away — no-op */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[FournisseursSectionVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddFournisseur()
    {
        try
        {
            ErrorMessage = null;
            var dialog = new NouveauFournisseurDialog
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
            Debug.WriteLine($"[FournisseursSectionVM] OpenAddFournisseur failed: {ex}");
        }
    }

    private async Task OpenEditFournisseurAsync(FournisseurCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dto = await _service.GetFournisseurByIdAsync(item.FournisseurId).ConfigureAwait(false);
            if (dto is null)
            {
                ErrorMessage = "Fournisseur introuvable.";
                return;
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new EditFournisseurDialog(dto)
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
            Debug.WriteLine($"[FournisseursSectionVM] OpenEditFournisseur failed: {ex}");
        }
    }

    private async Task OpenDetailFournisseurAsync(FournisseurCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dto = await _service.GetFournisseurByIdAsync(item.FournisseurId).ConfigureAwait(false);
            if (dto is null) { ErrorMessage = "Fournisseur introuvable."; return; }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var detail = new DetailFournisseurDialog(dto)
                {
                    Owner = Application.Current?.MainWindow
                };
                detail.ShowDialog();

                if (detail.ShouldEdit)
                {
                    var edit = new EditFournisseurDialog(dto)
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
            Debug.WriteLine($"[FournisseursSectionVM] OpenDetailFournisseur failed: {ex}");
        }
    }

    private async Task DeleteFournisseurAsync(FournisseurCardItem? item)
    {
        if (item is null) return;

        var confirm = MessageBox.Show(
            $"Supprimer « {item.Societe} » ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            ErrorMessage   = null;
            SuccessMessage = null;

            var ok = await _service.DeleteFournisseurAsync(item.FournisseurId).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                {
                    Fournisseurs.Remove(item);
                    SuccessMessage = LanguageManager.Instance["FourDeleteSuccess"];
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["FourDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[FournisseursSectionVM] Delete failed: {ex}");
        }
    }
}
