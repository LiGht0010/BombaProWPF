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

    #region Tab Navigation

    private void OnDepensesTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        DepensesTabButton.BackgroundColor = Color.FromArgb("#E53935");
        DepensesTabButton.TextColor = Colors.White;
        DepensesTabButton.FontAttributes = FontAttributes.Bold;

        CategoriesTabButton.BackgroundColor = Color.FromArgb("#C62828");
        CategoriesTabButton.TextColor = Color.FromArgb("#FFCDD2");
        CategoriesTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        DepensesTab.IsVisible = true;
        CategoriesTab.IsVisible = false;
    }

    private void OnCategoriesTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        CategoriesTabButton.BackgroundColor = Color.FromArgb("#E53935");
        CategoriesTabButton.TextColor = Colors.White;
        CategoriesTabButton.FontAttributes = FontAttributes.Bold;

        DepensesTabButton.BackgroundColor = Color.FromArgb("#C62828");
        DepensesTabButton.TextColor = Color.FromArgb("#FFCDD2");
        DepensesTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        DepensesTab.IsVisible = false;
        CategoriesTab.IsVisible = true;

        // Load categories if not already loaded
        if (_viewModel.CategoriesList.Count == 0)
        {
            _ = _viewModel.LoadCategoriesListAsync();
        }
    }

    #endregion

    #region Depense CRUD Handlers

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

    #endregion

    #region Category CRUD Handlers

    private async void OnAddCategorieClicked(object sender, EventArgs e)
    {
        var popup = new DepenseCategorieCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is DepenseCategorieDto newCategorie)
        {
            await _viewModel.CreateCategorieAsync(newCategorie);
        }
    }

    private async void OnEditCategorieClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepenseCategorieDto categorie)
        {
            var popup = new DepenseCategorieCreatePopup(categorie);
            var result = await this.ShowPopupAsync(popup);

            if (result is DepenseCategorieDto updatedCategorie)
            {
                await _viewModel.UpdateCategorieAsync(updatedCategorie);
            }
        }
    }

    private async void OnDeleteCategorieClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DepenseCategorieDto categorie)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment désactiver la catégorie '{categorie.Nom}'?\nCette action peut être annulée.",
                "Oui, désactiver",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteCategorieAsync(categorie);
            }
        }
    }

    #endregion
}