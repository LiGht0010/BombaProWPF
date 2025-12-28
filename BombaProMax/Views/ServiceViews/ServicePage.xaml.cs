using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ServiceViews;

public partial class ServicePage : ContentPage
{
    private readonly ServiceViewModel _viewModel;

    public ServicePage(ServiceViewModel viewModel)
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

    #region Tab Navigation

    private void OnServicesTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        ServicesTabButton.BackgroundColor = Color.FromArgb("#0097A7");
        ServicesTabButton.TextColor = Colors.White;
        ServicesTabButton.FontAttributes = FontAttributes.Bold;

        CategoriesTabButton.BackgroundColor = Color.FromArgb("#00838F");
        CategoriesTabButton.TextColor = Color.FromArgb("#B2EBF2");
        CategoriesTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        ServicesTab.IsVisible = true;
        CategoriesTab.IsVisible = false;
    }

    private void OnCategoriesTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        CategoriesTabButton.BackgroundColor = Color.FromArgb("#0097A7");
        CategoriesTabButton.TextColor = Colors.White;
        CategoriesTabButton.FontAttributes = FontAttributes.Bold;

        ServicesTabButton.BackgroundColor = Color.FromArgb("#00838F");
        ServicesTabButton.TextColor = Color.FromArgb("#B2EBF2");
        ServicesTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        ServicesTab.IsVisible = false;
        CategoriesTab.IsVisible = true;

        // Load categories if not already loaded
        if (_viewModel.CategoriesList.Count == 0)
        {
            _ = _viewModel.LoadCategoriesListAsync();
        }
    }

    #endregion

    #region Service CRUD Handlers

    private async void OnAddServiceClicked(object sender, EventArgs e)
    {
        var popup = new ServiceCreatePopup(_viewModel.Categories.ToList());
        var result = await this.ShowPopupAsync(popup);

        if (result is ServiceDto newService)
        {
            await _viewModel.CreateServiceAsync(newService);
        }
    }

    private async void OnEditServiceClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ServiceDto service)
        {
            var popup = new ServiceCreatePopup(_viewModel.Categories.ToList(), service);
            var result = await this.ShowPopupAsync(popup);

            if (result is ServiceDto updatedService)
            {
                await _viewModel.UpdateServiceAsync(updatedService);
            }
        }
    }

    private async void OnDeleteServiceClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ServiceDto service)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer le service '{service.Description}'?\nPrix: {service.Prix:N2} DH",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteServiceAsync(service);
            }
        }
    }

    #endregion

    #region Category CRUD Handlers

    private async void OnAddCategorieClicked(object sender, EventArgs e)
    {
        var popup = new ServiceCategorieCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is ServiceCategorieDto newCategorie)
        {
            await _viewModel.CreateCategorieAsync(newCategorie);
        }
    }

    private async void OnEditCategorieClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ServiceCategorieDto categorie)
        {
            var popup = new ServiceCategorieCreatePopup(categorie);
            var result = await this.ShowPopupAsync(popup);

            if (result is ServiceCategorieDto updatedCategorie)
            {
                await _viewModel.UpdateCategorieAsync(updatedCategorie);
            }
        }
    }

    private async void OnDeleteCategorieClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ServiceCategorieDto categorie)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment desactiver la categorie '{categorie.Nom}'?\nCette action peut etre annulee.",
                "Oui, desactiver",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteCategorieAsync(categorie);
            }
        }
    }

    #endregion
}