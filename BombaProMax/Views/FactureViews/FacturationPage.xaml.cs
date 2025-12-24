using BombaProMax.ViewModels;
using BombaProMax.Views.ClientViews;

namespace BombaProMax.Views.FactureViews;

public partial class FacturationPage : ContentPage
{
    private readonly FactureViewModel _viewModel;

    public FacturationPage(FactureViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Data loading is now handled by the QueryProperty setter in ViewModel
        // Only call LoadData if client is already loaded (for refresh)
        if (_viewModel.CurrentClient != null)
        {
            _viewModel.LoadDataCommand.Execute(null);
        }
    }

    // ============================
    // CHECKBOX SELECTION HANDLER
    // ============================

    /// <summary>
    /// Handles CheckBox CheckedChanged event to recalculate selection.
    /// This ensures CanProcess is updated when user directly clicks the checkbox.
    /// </summary>
    private void OnTransactionCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        // The binding already updates IsSelected on the DTO
        // We just need to trigger the ViewModel to recalculate selection totals
        _viewModel.RecalculateSelectionCommand?.Execute(null);
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

    private async void OnNavigateToClientsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ClientPage");
    }
}