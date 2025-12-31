using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.CaisseViews;

public partial class CaissePage : ContentPage
{
    private readonly CaisseViewModel _viewModel;

    public CaissePage(CaisseViewModel viewModel)
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

    private async void OnAddDepotClicked(object sender, EventArgs e)
    {
        var popup = new DepotCaisseCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is DepotCaisseDto newDepot)
        {
            await _viewModel.CreateDepotAsync(newDepot);
        }
    }

    private async void OnViewDepotClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepotCaisseDto depot)
        {
            await ShowDepotDetailsAsync(depot);
        }
    }

    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is DepotCaisseDto depot)
        {
            await ShowDepotDetailsAsync(depot);
        }
    }

    private async Task ShowDepotDetailsAsync(DepotCaisseDto depot)
    {
        var popup = new DepotCaisseDetailsPopup(depot);
        var result = await this.ShowPopupAsync(popup);

        // Check if user clicked "Edit" in the details popup
        if (result is ValueTuple<string, DepotCaisseDto> tuple && tuple.Item1 == "edit")
        {
            var editPopup = new DepotCaisseEditPopup(tuple.Item2);
            var editResult = await this.ShowPopupAsync(editPopup);

            if (editResult is DepotCaisseDto updatedDepot)
            {
                await _viewModel.UpdateDepotAsync(updatedDepot);
            }
        }
    }

    private async void OnEditDepotClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepotCaisseDto depot)
        {
            var popup = new DepotCaisseEditPopup(depot);
            var result = await this.ShowPopupAsync(popup);

            if (result is DepotCaisseDto updatedDepot)
            {
                await _viewModel.UpdateDepotAsync(updatedDepot);
            }
        }
    }

    private async void OnDeleteDepotClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepotCaisseDto depot)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer ce depot?\nMontant: {depot.Montant:N2} MAD\nDate: {depot.DateDisplay}",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteDepotAsync(depot);
            }
        }
    }
}
