using BombaProMax.Models;
using BombaProMax.ViewModels;
using BombaProMax.Views.FactureViews;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ClientViews;

public partial class ClientCreditManagement : ContentPage
{
    private readonly ClientCreditManagementViewModel _viewModel;

    public ClientCreditManagement()
    {
        InitializeComponent();
        _viewModel = new ClientCreditManagementViewModel();
        BindingContext = _viewModel;
        
        // Subscribe to property changes to update filter button styles
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public ClientCreditManagement(ClientCreditManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Subscribe to property changes to update filter button styles
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    /// <summary>
    /// Handles property changes from the ViewModel to update UI elements.
    /// </summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update CT filter button styles when filter properties change
        if (e.PropertyName == nameof(_viewModel.IsCTFilterAvailable) ||
            e.PropertyName == nameof(_viewModel.IsCTFilterInBL) ||
            e.PropertyName == nameof(_viewModel.IsCTFilterInvoiced))
        {
            UpdateCTFilterButtonStyles();
        }
        
        // Update BL filter button styles when filter properties change
        if (e.PropertyName == nameof(_viewModel.IsBlFilterAll) ||
            e.PropertyName == nameof(_viewModel.IsBlFilterInvoiced) ||
            e.PropertyName == nameof(_viewModel.IsBlFilterNotInvoiced))
        {
            UpdateBLFilterButtonStyles();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Data loading is now handled by the QueryProperty setter in ViewModel
        // Only initialize supporting data if client is already loaded
        if (_viewModel.CurrentClient != null)
        {
            await _viewModel.InitializeAsync();
        }
        
        // Initialize filter button styles
        UpdateCTFilterButtonStyles();
    }

    // ============================
    // NAVIGATION
    // ============================

    private async void OnBackToClientsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ClientPage");
    }

    // ============================
    // TAB SWITCHING (4 TABS)
    // ============================

    private void SetAllTabsInactive()
    {
        // Reset all tab buttons to inactive state
        TransactionsTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        TransactionsTabButton.TextColor = Color.FromArgb("#666666");
        TransactionsTabButton.FontAttributes = FontAttributes.None;

        ReglementsTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        ReglementsTabButton.TextColor = Color.FromArgb("#666666");
        ReglementsTabButton.FontAttributes = FontAttributes.None;

        FacturesTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        FacturesTabButton.TextColor = Color.FromArgb("#666666");
        FacturesTabButton.FontAttributes = FontAttributes.None;

        BLTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        BLTabButton.TextColor = Color.FromArgb("#666666");
        BLTabButton.FontAttributes = FontAttributes.None;

        // Hide all tabs
        TransactionsTab.IsVisible = false;
        ReglementsTab.IsVisible = false;
        FacturesTab.IsVisible = false;
        BLTab.IsVisible = false;
    }

    private void SetTabActive(Button tabButton, VerticalStackLayout tabContent)
    {
        tabButton.BackgroundColor = Color.FromArgb("#2196F3");
        tabButton.TextColor = Colors.White;
        tabButton.FontAttributes = FontAttributes.Bold;
        tabContent.IsVisible = true;
    }

    private void OnTransactionsTabClicked(object? sender, EventArgs e)
    {
        SetAllTabsInactive();
        SetTabActive(TransactionsTabButton, TransactionsTab);
        UpdateCTFilterButtonStyles();
    }

    private void OnReglementsTabClicked(object? sender, EventArgs e)
    {
        SetAllTabsInactive();
        SetTabActive(ReglementsTabButton, ReglementsTab);
    }

    private void OnFacturesTabClicked(object? sender, EventArgs e)
    {
        SetAllTabsInactive();
        SetTabActive(FacturesTabButton, FacturesTab);
    }

    private void OnBLTabClicked(object? sender, EventArgs e)
    {
        SetAllTabsInactive();
        SetTabActive(BLTabButton, BLTab);
        UpdateBLFilterButtonStyles();
    }

    // ============================
    // TRANSACTION ACTIONS
    // ============================

    private async void OnAddTransactionClicked(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentClient == null)
        {
            await DisplayAlert("Erreur", "Aucun client sélectionné", "OK");
            return;
        }

        try
        {
            var numero = await _viewModel.GenerateTransactionNumeroAsync();
            var popup = new CreditTransactionPopup(
                _viewModel.CurrentClient,
                _viewModel.Produits.ToList(),
                _viewModel.Services.ToList(),
                null,
                numero);

            var result = await this.ShowPopupAsync(popup);
            if (result is CreditTransactionDto transaction)
            {
                await _viewModel.AddTransactionAsync(transaction);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur lors de l'ajout de la transaction: {ex.Message}", "OK");
        }
    }

    private async void OnEditTransactionClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CreditTransactionDto transaction)
        {
            if (_viewModel.CurrentClient == null) return;

            try
            {
                var popup = new CreditTransactionPopup(
                    _viewModel.CurrentClient,
                    _viewModel.Produits.ToList(),
                    _viewModel.Services.ToList(),
                    transaction);

                var result = await this.ShowPopupAsync(popup);
                if (result is CreditTransactionDto updatedTransaction)
                {
                    await _viewModel.UpdateTransactionAsync(updatedTransaction);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la modification: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteTransactionClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CreditTransactionDto transaction)
        {
            var confirm = await DisplayAlert(
                "Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer cette transaction de {transaction.MontantTotal:N2} MAD?",
                "Oui",
                "Non");

            if (confirm)
            {
                await _viewModel.DeleteTransactionAsync(transaction);
            }
        }
    }

    // ============================
    // CT CHECKBOX & FILTER HELPERS
    // ============================

    /// <summary>
    /// Handles CT CheckBox CheckedChanged event to recalculate selection.
    /// </summary>
    private void OnCTCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _viewModel.RecalculateCTSelectionCommand?.Execute(null);
        UpdateCTFilterButtonStyles();
    }

    /// <summary>
    /// Updates CT filter button styles based on current filter mode.
    /// </summary>
    private void UpdateCTFilterButtonStyles()
    {
        CTFilterAvailableBtn.BackgroundColor = _viewModel.IsCTFilterAvailable 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        CTFilterAvailableBtn.TextColor = _viewModel.IsCTFilterAvailable 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        CTFilterAvailableBtn.FontAttributes = _viewModel.IsCTFilterAvailable 
            ? FontAttributes.Bold 
            : FontAttributes.None;

        CTFilterInBLBtn.BackgroundColor = _viewModel.IsCTFilterInBL 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        CTFilterInBLBtn.TextColor = _viewModel.IsCTFilterInBL 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        CTFilterInBLBtn.FontAttributes = _viewModel.IsCTFilterInBL 
            ? FontAttributes.Bold 
            : FontAttributes.None;

        CTFilterInvoicedBtn.BackgroundColor = _viewModel.IsCTFilterInvoiced 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        CTFilterInvoicedBtn.TextColor = _viewModel.IsCTFilterInvoiced 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        CTFilterInvoicedBtn.FontAttributes = _viewModel.IsCTFilterInvoiced 
            ? FontAttributes.Bold 
            : FontAttributes.None;
    }

    // ============================
    // REGLEMENT ACTIONS
    // ============================

    private async void OnAddReglementClicked(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentClient == null)
        {
            await DisplayAlert("Erreur", "Aucun client sélectionné", "OK");
            return;
        }

        try
        {
            var currentBalance = _viewModel.Bilan?.Balance ?? 0;
            var popup = new ReglementCreditPopup(
                _viewModel.CurrentClient,
                _viewModel.MoyensPaiement.ToList(),
                currentBalance);

            var result = await this.ShowPopupAsync(popup);
            if (result is ReglementCreditDto reglement)
            {
                await _viewModel.AddReglementAsync(reglement);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur lors de l'ajout du règlement: {ex.Message}", "OK");
        }
    }

    private async void OnEditReglementClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ReglementCreditDto reglement)
        {
            if (_viewModel.CurrentClient == null) return;

            try
            {
                var currentBalance = _viewModel.Bilan?.Balance ?? 0;
                var popup = new ReglementCreditPopup(
                    _viewModel.CurrentClient,
                    _viewModel.MoyensPaiement.ToList(),
                    currentBalance,
                    reglement);

                var result = await this.ShowPopupAsync(popup);
                if (result is ReglementCreditDto updatedReglement)
                {
                    await _viewModel.UpdateReglementAsync(updatedReglement);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la modification: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteReglementClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ReglementCreditDto reglement)
        {
            var confirm = await DisplayAlert(
                "Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer ce règlement de {reglement.MontantPaye:N2} MAD?",
                "Oui",
                "Non");

            if (confirm)
            {
                await _viewModel.DeleteReglementAsync(reglement);
            }
        }
    }

    // ============================
    // FACTURE CHECKBOX & ACTIONS
    // ============================

    /// <summary>
    /// Handles Facture CheckBox CheckedChanged event to recalculate selection.
    /// </summary>
    private void OnFactureCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _viewModel.RecalculateFactureSelectionCommand?.Execute(null);
    }

    /// <summary>
    /// Shows facture details popup.
    /// </summary>
    private async void OnFactureDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FactureDto facture)
        {
            try
            {
                var popup = new FactureDetails(facture);
                await this.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing facture details: {ex.Message}");
                await DisplayAlert("Erreur", $"Impossible d'afficher les détails: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Deletes a facture.
    /// </summary>
    private async void OnDeleteFactureClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FactureDto facture)
        {
            await _viewModel.DeleteFactureCommand.ExecuteAsync(facture);
        }
    }

    // ============================
    // BL CHECKBOX & ACTIONS
    // ============================

    /// <summary>
    /// Handles BL CheckBox CheckedChanged event to recalculate selection.
    /// </summary>
    private void OnBLCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _viewModel.RecalculateBLSelectionCommand?.Execute(null);
    }

    /// <summary>
    /// Shows BL details popup.
    /// </summary>
    private async void OnBLDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BonLivraisonDto bl)
        {
            try
            {
                var popup = new BLDetails(bl);
                await this.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing BL details: {ex.Message}");
                await DisplayAlert("Erreur", $"Impossible d'afficher les détails: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Deletes a BL.
    /// </summary>
    private async void OnDeleteBLClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BonLivraisonDto bl)
        {
            await _viewModel.DeleteBLCommand.ExecuteAsync(bl);
            UpdateBLFilterButtonStyles();
        }
    }

    // ============================
    // BL FILTER BUTTON STYLE HELPERS
    // ============================

    private void UpdateBLFilterButtonStyles()
    {
        BLFilterAllBtn.BackgroundColor = _viewModel.IsBlFilterAll 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        BLFilterAllBtn.TextColor = _viewModel.IsBlFilterAll 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        BLFilterAllBtn.FontAttributes = _viewModel.IsBlFilterAll 
            ? FontAttributes.Bold 
            : FontAttributes.None;

        BLFilterInvoicedBtn.BackgroundColor = _viewModel.IsBlFilterInvoiced 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        BLFilterInvoicedBtn.TextColor = _viewModel.IsBlFilterInvoiced 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        BLFilterInvoicedBtn.FontAttributes = _viewModel.IsBlFilterInvoiced 
            ? FontAttributes.Bold 
            : FontAttributes.None;

        BLFilterNotInvoicedBtn.BackgroundColor = _viewModel.IsBlFilterNotInvoiced 
            ? Color.FromArgb("#4A8FBF") 
            : Color.FromArgb("#E8ECF1");
        BLFilterNotInvoicedBtn.TextColor = _viewModel.IsBlFilterNotInvoiced 
            ? Colors.White 
            : Color.FromArgb("#5A6068");
        BLFilterNotInvoicedBtn.FontAttributes = _viewModel.IsBlFilterNotInvoiced 
            ? FontAttributes.Bold 
            : FontAttributes.None;
    }
}