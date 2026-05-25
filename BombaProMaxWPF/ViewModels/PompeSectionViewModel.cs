using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>Card-level wrapper with derived display properties for one pump.</summary>
public partial class PompeCardItem : ObservableObject
{
    public PompeDto Pompe { get; }

    public PompeCardItem(PompeDto pompe) => Pompe = pompe;

    public string Numero => Pompe.Numero ?? string.Empty;

    public string CarburantDisplay =>
        string.IsNullOrWhiteSpace(Pompe.CarburantNom)
            ? string.Empty
            : $"{Pompe.ReservoirNumero} / {Pompe.CarburantNom}";

    public string CompteurElectroniqueDisplay =>
        Pompe.CompteurElectroniqueActuel.HasValue
            ? Pompe.CompteurElectroniqueActuel.Value.ToString("N1")
            : "—";

    public string CompteurMecaniqueDisplay =>
        Pompe.CompteurMecaniqueActuel.HasValue
            ? Pompe.CompteurMecaniqueActuel.Value.ToString("N1")
            : "—";

    public string Statut => Pompe.Statut ?? string.Empty;

    public bool IsActive =>
        Statut.Equals("Actif", StringComparison.OrdinalIgnoreCase) ||
        Statut.Equals("Active", StringComparison.OrdinalIgnoreCase);

    public bool IsEnMaintenance =>
        Statut.Contains("Maintenance", StringComparison.OrdinalIgnoreCase);

    public bool IsArretee => !IsActive && !IsEnMaintenance;
}

/// <summary>ViewModel for the Pompes dashboard section.</summary>
public partial class PompeSectionViewModel : ObservableObject
{
    private readonly PompeService _pompeService = new();

    public ObservableCollection<PompeCardItem> Cards { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _activesCount;
    [ObservableProperty] private int _maintenanceCount;
    [ObservableProperty] private int _arreteesCount;

    private bool _loaded;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var pompes = await _pompeService.GetAllAsync();
            Cards.Clear();
            foreach (var p in pompes)
                Cards.Add(new PompeCardItem(p));
            RecalcStats();
            _loaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddPompeAsync()
    {
        // TODO: open WPF add pompe dialog when created
        MessageBox.Show("Fonctionnalité disponible prochainement.", "Nouvelle pompe",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task EditPompeAsync(PompeCardItem? item)
    {
        if (item == null) return;
        // TODO: open WPF edit pompe dialog when created
        MessageBox.Show($"Modification de {item.Numero} — disponible prochainement.", "Modifier",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DetailPompeAsync(PompeCardItem? item)
    {
        if (item == null) return;
        // TODO: open WPF detail pompe dialog when created
        MessageBox.Show($"Détails de {item.Numero} — disponible prochainement.", "Détails",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeletePompeAsync(PompeCardItem? item)
    {
        if (item == null) return;
        var result = MessageBox.Show(
            $"Supprimer la pompe '{item.Numero}' ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        var success = await _pompeService.DeleteAsync(item.Pompe.ID);
        if (success)
        {
            Cards.Remove(item);
            RecalcStats();
        }
    }

    private void RecalcStats()
    {
        TotalCount = Cards.Count;
        ActivesCount = Cards.Count(c => c.IsActive);
        MaintenanceCount = Cards.Count(c => c.IsEnMaintenance);
        ArreteesCount = Cards.Count(c => c.IsArretee);
    }
}
