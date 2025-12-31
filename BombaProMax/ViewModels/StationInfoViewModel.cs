using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BombaProMax.ViewModels;

/// <summary>
/// ViewModel for managing station information (business details for official documents).
/// </summary>
public partial class StationInfoViewModel : ObservableObject
{
    private readonly StationInfoService _stationInfoService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private StationInfoDto? _stationInfo;

    [ObservableProperty]
    private bool _hasStationInfo;

    [ObservableProperty]
    private string _statusMessage = "";

    public StationInfoViewModel(StationInfoService stationInfoService, IDialogService dialogService)
    {
        _stationInfoService = stationInfoService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    public async Task LoadStationInfoAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Chargement...";

            var info = await _stationInfoService.GetStationInfoAsync(forceRefresh: true);
            StationInfo = info;
            HasStationInfo = info != null && info.ID > 0;
            
            StatusMessage = HasStationInfo ? "" : "Aucune information configurée";
        }
        catch (Exception ex)
        {
            StatusMessage = "Erreur de chargement";
            System.Diagnostics.Debug.WriteLine($"Error loading station info: {ex.Message}");
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les informations: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EditStationInfoAsync()
    {
        try
        {
            var result = await _dialogService.ShowStationInfoEditPopupAsync(StationInfo);
            if (result != null)
            {
                StationInfo = result;
                HasStationInfo = true;
                await _dialogService.ShowAlertAsync("Succès", "Informations enregistrées avec succès");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error editing station info: {ex.Message}");
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de modifier les informations: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateStationInfoAsync()
    {
        try
        {
            var result = await _dialogService.ShowStationInfoEditPopupAsync(null);
            if (result != null)
            {
                StationInfo = result;
                HasStationInfo = true;
                await _dialogService.ShowAlertAsync("Succès", "Informations créées avec succès");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating station info: {ex.Message}");
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de créer les informations: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PickLogoAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Sélectionner un logo",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var logoBytes = memoryStream.ToArray();

                // Update logo via service
                var success = await _stationInfoService.UpdateLogoAsync(logoBytes);
                if (success)
                {
                    // Refresh station info to get updated logo
                    await LoadStationInfoAsync();
                    await _dialogService.ShowAlertAsync("Succès", "Logo mis à jour avec succès");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Erreur", "Impossible de mettre à jour le logo");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking logo: {ex.Message}");
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la sélection du logo: {ex.Message}");
        }
    }
}
