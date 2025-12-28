using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteServiceViews;

public partial class VenteServicePage : ContentPage
{
    private readonly VenteServiceViewModel _viewModel;

    public VenteServicePage(VenteServiceViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnAddVenteClicked(object sender, EventArgs e)
    {
        var popup = new VenteServiceCreatePopup(
            _viewModel.AvailableServices.ToList(),
            _viewModel.Categories.ToList(),
            _viewModel.MoyensPaiement.ToList());
        var result = await this.ShowPopupAsync(popup);

        if (result is VenteServiceDto newVente)
        {
            await _viewModel.CreateVenteAsync(newVente);
        }
    }

    private async void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is VenteServiceDto vente)
        {
            await ShowDetailsPopup(vente);
        }
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VenteServiceDto vente)
        {
            await ShowDetailsPopup(vente);
        }
    }

    private async Task ShowDetailsPopup(VenteServiceDto vente)
    {
        var popup = new VenteServiceDetailsPopup(vente);
        var result = await this.ShowPopupAsync(popup);

        // If user clicked "Edit" from details popup, open edit popup
        if (result is VenteServiceDto venteToEdit && popup.RequestedEdit)
        {
            await OpenEditPopup(venteToEdit);
        }
    }

    private async void OnEditVenteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VenteServiceDto vente)
        {
            await OpenEditPopup(vente);
        }
    }

    private async Task OpenEditPopup(VenteServiceDto vente)
    {
        var popup = new VenteServiceEditPopup(
            vente,
            _viewModel.AvailableServices.ToList(),
            _viewModel.Categories.ToList(),
            _viewModel.MoyensPaiement.ToList(),
            _viewModel.Clients.ToList());
        var result = await this.ShowPopupAsync(popup);

        if (result is VenteServiceDto updatedVente)
        {
            await _viewModel.UpdateVenteAsync(updatedVente);
        }
    }

    private async void OnDeleteVenteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VenteServiceDto vente)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer la vente {vente.NumeroVente}?\nMontant: {vente.MontantTotal:N2} DH",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteVenteAsync(vente);
            }
        }
    }
}
