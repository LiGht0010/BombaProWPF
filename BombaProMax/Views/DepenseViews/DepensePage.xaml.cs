using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DepenseViews;

public partial class DepensePage : ContentPage
{
    private readonly DepenseViewModel _viewModel;

    public DepensePage(DepenseViewModel viewModel)
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

    private async void OnAddDepenseClicked(object sender, EventArgs e)
    {
        var popup = new DepenseCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is DepenseDto newDepense)
        {
            await _viewModel.CreateDepenseAsync(newDepense);
        }
    }

    private async void OnEditDepenseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepenseDto depense)
        {
            var popup = new DepenseCreatePopup(depense);
            var result = await this.ShowPopupAsync(popup);

            if (result is DepenseDto updatedDepense)
            {
                await _viewModel.UpdateDepenseAsync(updatedDepense);
            }
        }
    }

    private async void OnDeleteDepenseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepenseDto depense)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer la dépense {depense.Numero}?\nMontant: {depense.Montant:N2} MAD",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteDepenseAsync(depense);
            }
        }
    }
}