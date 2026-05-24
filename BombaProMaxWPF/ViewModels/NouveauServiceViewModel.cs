using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Nouveau Service creation dialog.
/// </summary>
public partial class NouveauServiceViewModel : ObservableObject
{
    private readonly ServiceService _serviceService = new();
    private readonly ServiceCategorieService _categorieService = new();

    // ── Form fields ──────────────────────────────────────────────────
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private ServiceCategorieDto? _selectedCategorie;
    [ObservableProperty] private string? _prixText;

    // ── UI state ─────────────────────────────────────────────────────
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isLoadingCategories;
    [ObservableProperty] private string? _errorMessage;

    /// <summary>Set to true on successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public ObservableCollection<ServiceCategorieDto> Categories { get; } = new();

    public IAsyncRelayCommand LoadCategoriesCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public NouveauServiceViewModel()
    {
        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoadingCategories = true;
            var list = await _categorieService.GetAllCategoriesAsync().ConfigureAwait(false);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Categories.Clear();
                foreach (var c in list)
                    Categories.Add(c);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NouveauServiceVM] LoadCategories failed: {ex}");
        }
        finally
        {
            IsLoadingCategories = false;
        }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = LanguageManager.Instance["NouveauSrvValidationNumero"];
            return;
        }

        decimal? prix = null;
        if (!string.IsNullOrWhiteSpace(PrixText))
        {
            if (decimal.TryParse(PrixText, NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                prix = p;
        }

        try
        {
            IsSaving = true;

            var dto = new ServiceDto
            {
                Numero = Numero.Trim(),
                Description = Description?.Trim(),
                Prix = prix,
                ServiceCategorieID = SelectedCategorie?.ID
            };

            var created = await _serviceService.CreateServiceAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (created is not null)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["NouveauSrvSaveError"];
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[NouveauServiceVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}
