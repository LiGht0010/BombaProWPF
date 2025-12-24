using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BombaProMax.Views.AchatViews;

public partial class AchatAllocationPopup : Popup
{
    private readonly AchatAllocationService _allocationService;
    private readonly AchatDto _achat;
    private readonly decimal _quantiteRestante;

    public ObservableCollection<ReservoirAllocationViewModel> Reservoirs { get; } = [];

    public AchatAllocationPopup(
        AchatAllocationService allocationService,
        AchatDto achat,
        decimal quantiteRestante)
    {
        InitializeComponent();

        _allocationService = allocationService;
        _achat = achat;
        _quantiteRestante = quantiteRestante;

        // Set achat info
        AchatNumeroLabel.Text = achat.Numero ?? $"#{achat.ID}";
        ProduitNomLabel.Text = achat.ProduitNom ?? "N/A";
        QuantiteRestanteLabel.Text = $"{quantiteRestante:N0} L";
        TotalRequiredLabel.Text = $"/ {quantiteRestante:N0} L";

        ReservoirsCollectionView.ItemsSource = Reservoirs;

        // Load reservoirs
        _ = LoadReservoirsAsync();
    }

    private async Task LoadReservoirsAsync()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            if (!_achat.ProduitID.HasValue)
            {
                ShowError("Produit non spécifié pour cet achat");
                return;
            }

            var reservoirs = await _allocationService.GetAvailableReservoirsAsync(_achat.ProduitID.Value);

            Reservoirs.Clear();
            foreach (var reservoir in reservoirs)
            {
                var vm = new ReservoirAllocationViewModel(reservoir);
                vm.PropertyChanged += OnReservoirPropertyChanged;
                Reservoirs.Add(vm);
            }

            UpdateSummary();
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void OnReservoirPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReservoirAllocationViewModel.QuantiteAllouee) ||
            e.PropertyName == nameof(ReservoirAllocationViewModel.IsSelected))
        {
            UpdateSummary();
        }
    }

    private void OnReservoirCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is ReservoirAllocationViewModel vm)
        {
            if (!e.Value)
            {
                vm.QuantiteAllouee = 0;
            }
        }
        UpdateSummary();
    }

    private void OnQuantityChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var totalAlloue = Reservoirs
            .Where(r => r.IsSelected && r.QuantiteAllouee > 0)
            .Sum(r => r.QuantiteAllouee);

        var restant = _quantiteRestante - totalAlloue;

        TotalAlloueLabel.Text = $"{totalAlloue:N0}";
        RestantLabel.Text = $"{restant:N0} L";

        // Validation
        var isValid = Math.Abs(restant) < 0.01m; // Allow small floating point differences
        var hasOverflow = Reservoirs.Any(r => r.IsSelected && r.QuantiteAllouee > r.EspaceDisponible);
        var hasNegative = Reservoirs.Any(r => r.IsSelected && r.QuantiteAllouee < 0);

        if (hasOverflow)
        {
            ValidationLabel.Text = "?? Dépassement";
            ValidationBadge.BackgroundColor = Color.FromArgb("#FDF4E8");
            ConfirmButton.IsEnabled = false;
            ShowError("Une ou plusieurs quantités dépassent l'espace disponible");
        }
        else if (hasNegative)
        {
            ValidationLabel.Text = "? Invalide";
            ValidationBadge.BackgroundColor = Color.FromArgb("#FDECEC");
            ConfirmButton.IsEnabled = false;
            ShowError("Les quantités ne peuvent pas être négatives");
        }
        else if (totalAlloue > _quantiteRestante)
        {
            ValidationLabel.Text = "?? Excédent";
            ValidationBadge.BackgroundColor = Color.FromArgb("#FDF4E8");
            ConfirmButton.IsEnabled = false;
            ShowError($"Total alloué ({totalAlloue:N0} L) dépasse la quantité disponible ({_quantiteRestante:N0} L)");
        }
        else if (isValid)
        {
            ValidationLabel.Text = "? Complet";
            ValidationBadge.BackgroundColor = Color.FromArgb("#E8F5ED");
            ConfirmButton.IsEnabled = true;
            HideError();
        }
        else if (totalAlloue > 0)
        {
            ValidationLabel.Text = "? Partiel";
            ValidationBadge.BackgroundColor = Color.FromArgb("#E8F1F8");
            ConfirmButton.IsEnabled = true; // Allow partial allocation
            HideError();
        }
        else
        {
            ValidationLabel.Text = "? Incomplet";
            ValidationBadge.BackgroundColor = Color.FromArgb("#FDECEC");
            ConfirmButton.IsEnabled = false;
            HideError();
        }

        // Update restant color
        if (restant < 0)
            RestantLabel.TextColor = Color.FromArgb("#E16C6C");
        else if (restant == 0)
            RestantLabel.TextColor = Color.FromArgb("#5EAA8D");
        else
            RestantLabel.TextColor = Color.FromArgb("#E8A84C");
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        try
        {
            var selectedReservoirs = Reservoirs
                .Where(r => r.IsSelected && r.QuantiteAllouee > 0)
                .ToList();

            if (selectedReservoirs.Count == 0)
            {
                ShowError("Veuillez sélectionner au moins un réservoir et spécifier une quantité");
                return;
            }

            // Validate each allocation
            foreach (var reservoir in selectedReservoirs)
            {
                if (reservoir.QuantiteAllouee > reservoir.EspaceDisponible)
                {
                    ShowError($"Réservoir {reservoir.Numero}: quantité dépasse l'espace disponible");
                    return;
                }
            }

            var totalAlloue = selectedReservoirs.Sum(r => r.QuantiteAllouee);

            // Create batch request
            var request = new BatchAllocationRequestDto
            {
                AchatID = _achat.ID,
                TotalQuantite = totalAlloue,
                Allocations = selectedReservoirs.Select(r => new AllocationItemDto
                {
                    ReservoirID = r.ID,
                    QuantiteAllouee = r.QuantiteAllouee
                }).ToList()
            };

            // Submit allocation
            var result = await _allocationService.BatchAllocateAsync(request);

            if (result?.Success == true)
            {
                await CloseAsync(result);
            }
            else
            {
                ShowError(result?.Message ?? "Erreur lors de l'allocation");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.IsVisible = false;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }
}

/// <summary>
/// ViewModel wrapper for reservoir allocation with selection and quantity binding.
/// </summary>
public class ReservoirAllocationViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private decimal _quantiteAllouee;

    public int ID { get; }
    public string Numero { get; }
    public int? ProduitID { get; }
    public string? ProduitNom { get; }
    public decimal Capacite { get; }
    public decimal NiveauActuel { get; }
    public decimal EspaceDisponible { get; }
    public decimal TauxRemplissage { get; }
    public bool EstVide { get; }
    public bool EstCompatible { get; }
    public double FillPercentage => (double)(Capacite > 0 ? NiveauActuel / Capacite : 0);

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public decimal QuantiteAllouee
    {
        get => _quantiteAllouee;
        set
        {
            if (_quantiteAllouee != value)
            {
                _quantiteAllouee = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QuantiteAllouee)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ReservoirAllocationViewModel(ReservoirAllocationInfoDto dto)
    {
        ID = dto.ID;
        Numero = dto.Numero;
        ProduitID = dto.ProduitID;
        ProduitNom = dto.ProduitNom;
        Capacite = dto.Capacite;
        NiveauActuel = dto.NiveauActuel;
        EspaceDisponible = dto.EspaceDisponible;
        TauxRemplissage = dto.TauxRemplissage;
        EstVide = dto.EstVide;
        EstCompatible = dto.EstCompatible;
    }
}