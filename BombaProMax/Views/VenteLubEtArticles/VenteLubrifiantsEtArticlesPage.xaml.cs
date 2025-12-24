using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteLubEtArticles;

public partial class VenteLubrifiantsEtArticlesPage : ContentPage
{
    private readonly VenteLubrifiantsEtArticlesViewModel _viewModel;

    public VenteLubrifiantsEtArticlesPage(VenteLubrifiantsEtArticlesViewModel viewModel)
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
        var popup = new VenteLubrifiantsEtArticlesCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is VenteLubrifiantsEtArticlesDto newVente)
        {
            await _viewModel.CreateVenteAsync(newVente);
        }
    }

    private async void OnEditVenteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VenteLubrifiantsEtArticlesDto vente)
        {
            var popup = new VenteLubrifiantsEtArticlesCreatePopup(vente);
            var result = await this.ShowPopupAsync(popup);

            if (result is VenteLubrifiantsEtArticlesDto updatedVente)
            {
                await _viewModel.UpdateVenteAsync(updatedVente);
            }
        }
    }

    private async void OnDeleteVenteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is VenteLubrifiantsEtArticlesDto vente)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer la vente {vente.NumeroVente}?\nLe stock du produit sera restauré.",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteVenteAsync(vente);
            }
        }
    }
}