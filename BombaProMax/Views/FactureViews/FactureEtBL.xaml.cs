using BombaProMax.Models;
using BombaProMax.ViewModels;
using BombaProMax.Views.ClientViews;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.FactureViews;

public partial class FactureEtBL : ContentPage
{
    private readonly FactureEtBLViewModel _viewModel;

    public FactureEtBL(FactureEtBLViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Data loading is now handled by the QueryProperty setter in ViewModel
        // Only call Initialize if data might need refresh
        if (_viewModel.CurrentClient != null)
        {
            await _viewModel.RefreshAllAsync();
        }
    }

    // ============================
    // NAVIGATION
    // ============================

    private async void OnNavigateToClientCreditClicked(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentClient != null)
        {
            await Shell.Current.GoToAsync($"{nameof(ClientCreditManagement)}?clientId={_viewModel.ClientId}");
        }
        else
        {
            await Shell.Current.GoToAsync("///ClientPage");
        }
    }

    private async void OnNavigateToFacturationClicked(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentClient != null)
        {
            await Shell.Current.GoToAsync($"{nameof(FacturationPage)}?clientId={_viewModel.ClientId}");
        }
        else
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un client", "OK");
        }
    }

    private async void OnNavigateToClientsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ClientPage");
    }

    // ============================
    // TAB SWITCHING
    // ============================

    private void OnFacturesTabClicked(object? sender, EventArgs e)
    {
        // Update tab button styles
        FacturesTabButton.BackgroundColor = Color.FromArgb("#4A8FBF");
        FacturesTabButton.TextColor = Colors.White;
        FacturesTabButton.FontAttributes = FontAttributes.Bold;

        BLTabButton.BackgroundColor = Color.FromArgb("#8FA3B8");
        BLTabButton.TextColor = Colors.White;
        BLTabButton.FontAttributes = FontAttributes.None;

        // Show/hide tabs
        FacturesTab.IsVisible = true;
        BLTab.IsVisible = false;
    }

    private void OnBLTabClicked(object? sender, EventArgs e)
    {
        // Update tab button styles
        BLTabButton.BackgroundColor = Color.FromArgb("#4A8FBF");
        BLTabButton.TextColor = Colors.White;
        BLTabButton.FontAttributes = FontAttributes.Bold;

        FacturesTabButton.BackgroundColor = Color.FromArgb("#8FA3B8");
        FacturesTabButton.TextColor = Colors.White;
        FacturesTabButton.FontAttributes = FontAttributes.None;

        // Show/hide tabs
        BLTab.IsVisible = true;
        FacturesTab.IsVisible = false;
    }

    // ============================
    // CHECKBOX SELECTION HANDLERS
    // ============================

    /// <summary>
    /// Handles Facture CheckBox CheckedChanged event to recalculate selection.
    /// </summary>
    private void OnFactureCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _viewModel.RecalculateFactureSelectionCommand?.Execute(null);
    }

    /// <summary>
    /// Handles BL CheckBox CheckedChanged event to recalculate selection.
    /// </summary>
    private void OnBLCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _viewModel.RecalculateBLSelectionCommand?.Execute(null);
    }

    // ============================
    // FACTURE ACTIONS
    // ============================

    private async void OnFactureDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FactureDto facture)
        {
            try
            {
                // Show the FactureDetails popup
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

    private async void OnDeleteFactureClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FactureDto facture)
        {
            await _viewModel.DeleteFactureCommand.ExecuteAsync(facture);
        }
    }

    // ============================
    // BON LIVRAISON ACTIONS
    // ============================

    private async void OnBLDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BonLivraisonDto bl)
        {
            try
            {
                // Show the BLDetails popup
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

    private async void OnDeleteBLClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is BonLivraisonDto bl)
        {
            await _viewModel.DeleteBLCommand.ExecuteAsync(bl);
            UpdateBLFilterButtonStyles();
        }
    }

    // ============================
    // FILTER BUTTON STYLE HELPERS
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