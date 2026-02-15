using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views;

public partial class StockWithdrawalCreatePopup : Popup
{
    private readonly StockWithdrawalService _withdrawalService;
    private List<ReservoirWithdrawalInfoDto> _reservoirs = [];
    private ReservoirWithdrawalInfoDto? _selectedReservoir;

    public StockWithdrawalCreatePopup()
    {
        InitializeComponent();
        _withdrawalService = new StockWithdrawalService();
        
        // Initialize date picker to today
        DateRetraitPicker.Date = DateTime.Today;
        DateRetraitPicker.MaximumDate = DateTime.Today;
        
        LoadReservoirsAsync();
    }

    private async void LoadReservoirsAsync()
    {
        try
        {
            _reservoirs = await _withdrawalService.GetReservoirsForWithdrawalAsync();
            _reservoirs = _reservoirs.Where(r => r.StockDisponible > 0).ToList();

            ReservoirPicker.ItemsSource = _reservoirs;
            ReservoirPicker.ItemDisplayBinding = new Binding("DisplayText");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StockWithdrawalPopup] Error loading reservoirs: {ex.Message}");
        }
    }

    private void OnReservoirChanged(object? sender, EventArgs e)
    {
        if (ReservoirPicker.SelectedIndex >= 0 && ReservoirPicker.SelectedIndex < _reservoirs.Count)
        {
            _selectedReservoir = _reservoirs[ReservoirPicker.SelectedIndex];
            
            // Update stock info display
            StockInfoBorder.IsVisible = true;
            StockDisponibleLabel.Text = _selectedReservoir.StockDisponible.ToString("N2");
            NombreLotsLabel.Text = _selectedReservoir.NombreLots.ToString();
            ProduitLabel.Text = _selectedReservoir.ProduitNom ?? "-";
            MaxButton.IsEnabled = true;

            // Clear quantity and validation
            QuantiteEntry.Text = "";
            ValidateForm();
        }
        else
        {
            _selectedReservoir = null;
            StockInfoBorder.IsVisible = false;
            MaxButton.IsEnabled = false;
            ValidateForm();
        }
    }

    private void OnQuantiteChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateForm();
    }

    private void OnMaxClicked(object? sender, EventArgs e)
    {
        if (_selectedReservoir != null)
        {
            QuantiteEntry.Text = _selectedReservoir.StockDisponible.ToString("F2");
        }
    }

    private void ValidateForm()
    {
        bool isValid = true;
        QuantiteValidationLabel.IsVisible = false;

        if (_selectedReservoir == null)
        {
            isValid = false;
        }
        else if (!string.IsNullOrWhiteSpace(QuantiteEntry.Text))
        {
            if (decimal.TryParse(QuantiteEntry.Text, out decimal quantite))
            {
                if (quantite <= 0)
                {
                    QuantiteValidationLabel.Text = "La quantite doit etre superieure a zero";
                    QuantiteValidationLabel.IsVisible = true;
                    isValid = false;
                }
                else if (quantite > _selectedReservoir.StockDisponible)
                {
                    QuantiteValidationLabel.Text = $"Quantite depasse le stock disponible ({_selectedReservoir.StockDisponible:N2} L)";
                    QuantiteValidationLabel.IsVisible = true;
                    isValid = false;
                }
            }
            else
            {
                QuantiteValidationLabel.Text = "Quantite invalide";
                QuantiteValidationLabel.IsVisible = true;
                isValid = false;
            }
        }
        else
        {
            isValid = false;
        }

        CreateButton.IsEnabled = isValid;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        if (_selectedReservoir == null)
        {
            ShowError("Veuillez selectionner un reservoir");
            return;
        }

        if (!decimal.TryParse(QuantiteEntry.Text, out decimal quantite) || quantite <= 0)
        {
            ShowError("Veuillez entrer une quantite valide");
            return;
        }

        if (quantite > _selectedReservoir.StockDisponible)
        {
            ShowError($"Quantite depasse le stock disponible ({_selectedReservoir.StockDisponible:N2} L)");
            return;
        }

        try
        {
            CreateButton.IsEnabled = false;
            CreateButton.Text = "En cours...";

            var currentUser = App.CurrentUser;
            var request = new StockWithdrawalRequestDto
            {
                ReservoirID = _selectedReservoir.ID,
                Quantite = quantite,
                DateRetrait = DateTime.SpecifyKind(DateRetraitPicker.Date, DateTimeKind.Utc),
                Motif = MotifEditor.Text,
                UtilisateurID = currentUser?.UserId,
                UtilisateurNom = currentUser?.Name
            };

            var result = await _withdrawalService.WithdrawStockAsync(request);

            if (result.Success)
            {
                Close(result);
            }
            else
            {
                ShowError(result.Message ?? "Erreur lors du retrait");
                CreateButton.IsEnabled = true;
                CreateButton.Text = "Retirer le Stock";
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
            CreateButton.IsEnabled = true;
            CreateButton.Text = "Retirer le Stock";
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnDateRetraitChanged(object? sender, DateChangedEventArgs e)
    {
        ValidateForm();
    }
}
