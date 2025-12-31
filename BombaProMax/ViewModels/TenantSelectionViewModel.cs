using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class TenantSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TenantInfo> _tenants = [];

    [ObservableProperty]
    private TenantInfo? _selectedTenant;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _currentTenantId = string.Empty;

    public TenantSelectionViewModel()
    {
        LoadTenants();
    }

    private void LoadTenants()
    {
        CurrentTenantId = Preferences.Get("TenantId", string.Empty);

        var tenantList = new List<TenantInfo>
        {
            new()
            {
                TenantId = "sidikacem",
                Name = "Sidi Kacem",
                Description = "Station Sidi Kacem",
                Initials = "SK",
                Color = "#1F4E45",
                IsSelected = CurrentTenantId == "sidikacem"
            },
            new()
            {
                TenantId = "qserkber",
                Name = "Qser Kber",
                Description = "Station Qser Kber",
                Initials = "QK",
                Color = "#2563EB",
                IsSelected = CurrentTenantId == "qserkber"
            },
            new()
            {
                TenantId = "sidiaddi",
                Name = "Sidi Addi",
                Description = "Station Sidi Addi",
                Initials = "SA",
                Color = "#DC2626",
                IsSelected = CurrentTenantId == "sidiaddi"
            }
        };

        Tenants = new ObservableCollection<TenantInfo>(tenantList);
        SelectedTenant = tenantList.FirstOrDefault(t => t.IsSelected);
    }

    /// <summary>
    /// Refreshes the selection state based on saved preferences.
    /// Call this from OnAppearing to update the checkmark when navigating back.
    /// </summary>
    public void RefreshSelection()
    {
        CurrentTenantId = Preferences.Get("TenantId", string.Empty);

        foreach (var tenant in Tenants)
        {
            tenant.IsSelected = tenant.TenantId == CurrentTenantId;
        }

        SelectedTenant = Tenants.FirstOrDefault(t => t.IsSelected);
    }

    [RelayCommand]
    private async Task SelectTenantAsync(TenantInfo tenant)
    {
        if (tenant == null) return;

        IsLoading = true;

        try
        {
            foreach (var t in Tenants)
            {
                t.IsSelected = t.TenantId == tenant.TenantId;
            }

            SelectedTenant = tenant;
            CurrentTenantId = tenant.TenantId;

            ApiConfig.SetAndSaveTenantId(tenant.TenantId);

            await Task.Delay(300);

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", $"Impossible de selectionner le client: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ContinueWithSelectedAsync()
    {
        if (SelectedTenant == null)
        {
            await Shell.Current.DisplayAlert("Selection requise", "Veuillez selectionner un profil client.", "OK");
            return;
        }

        await SelectTenantAsync(SelectedTenant);
    }
}
