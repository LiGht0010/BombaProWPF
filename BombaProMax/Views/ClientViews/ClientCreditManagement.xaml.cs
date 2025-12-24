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
    }

    public ClientCreditManagement(ClientCreditManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
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
    }

    // ============================
    // NAVIGATION
    // ============================

    private async void OnBackToClientsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ClientPage");
    }

    private async void OnNavigateToFactureEtBLClicked(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentClient != null)
        {
            await Shell.Current.GoToAsync($"{nameof(FactureEtBL)}?clientId={_viewModel.ClientId}");
        }
        else
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un client", "OK");
        }
    }

    // ============================
    // TAB SWITCHING
    // ============================

    private void OnTransactionsTabClicked(object? sender, EventArgs e)
    {
        TransactionsTabButton.BackgroundColor = Color.FromArgb("#2196F3");
        TransactionsTabButton.TextColor = Colors.White;
        TransactionsTabButton.FontAttributes = FontAttributes.Bold;

        ReglementsTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        ReglementsTabButton.TextColor = Color.FromArgb("#666666");
        ReglementsTabButton.FontAttributes = FontAttributes.None;

        TransactionsTab.IsVisible = true;
        ReglementsTab.IsVisible = false;
    }

    private void OnReglementsTabClicked(object? sender, EventArgs e)
    {
        ReglementsTabButton.BackgroundColor = Color.FromArgb("#2196F3");
        ReglementsTabButton.TextColor = Colors.White;
        ReglementsTabButton.FontAttributes = FontAttributes.Bold;

        TransactionsTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        TransactionsTabButton.TextColor = Color.FromArgb("#666666");
        TransactionsTabButton.FontAttributes = FontAttributes.None;

        ReglementsTab.IsVisible = true;
        TransactionsTab.IsVisible = false;
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
}