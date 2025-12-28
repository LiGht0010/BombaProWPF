using BombaProMax.Models;
using BombaProMax.Services;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeagePage : ContentPage
{
    private readonly JaugeageViewModel _viewModel;
    private readonly JaugeageService _jaugeageService;

    public JaugeagePage(JaugeageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _jaugeageService = new JaugeageService();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    #region Tab Navigation

    private void OnJaugeagesTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        JaugeagesTabButton.BackgroundColor = Color.FromArgb("#00796B");
        JaugeagesTabButton.TextColor = Colors.White;
        JaugeagesTabButton.FontAttributes = FontAttributes.Bold;

        CalibrationTabButton.BackgroundColor = Color.FromArgb("#00695C");
        CalibrationTabButton.TextColor = Color.FromArgb("#B2DFDB");
        CalibrationTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        JaugeagesTab.IsVisible = true;
        CalibrationTab.IsVisible = false;
    }

    private void OnCalibrationTabClicked(object sender, EventArgs e)
    {
        // Update tab button styles
        CalibrationTabButton.BackgroundColor = Color.FromArgb("#00796B");
        CalibrationTabButton.TextColor = Colors.White;
        CalibrationTabButton.FontAttributes = FontAttributes.Bold;

        JaugeagesTabButton.BackgroundColor = Color.FromArgb("#00695C");
        JaugeagesTabButton.TextColor = Color.FromArgb("#B2DFDB");
        JaugeagesTabButton.FontAttributes = FontAttributes.None;

        // Show/Hide tabs
        JaugeagesTab.IsVisible = false;
        CalibrationTab.IsVisible = true;
    }

    #endregion

    #region Jaugeage CRUD Handlers

    private async void OnAddJaugeageClicked(object sender, EventArgs e)
    {
        var popup = new JaugeageCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is JaugeageWithDetailsDto)
        {
            // Refresh the list
            await _viewModel.LoadHistoryAsync();
        }
    }

    private async void OnEditJaugeageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is JaugeageDto jaugeage)
        {
            var popup = new JaugeageEditPopup(jaugeage);
            var result = await this.ShowPopupAsync(popup);

            if (result is JaugeageDto or bool)
            {
                // Refresh the list
                await _viewModel.LoadHistoryAsync();
            }
        }
    }

    private async void OnDetailsJaugeageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is JaugeageDto jaugeage)
        {
            var popup = new JaugeageDetailsPopup(jaugeage);
            var result = await this.ShowPopupAsync(popup);

            // Check if user wants to edit from details popup
            if (result is { } obj)
            {
                var type = obj.GetType();
                var actionProp = type.GetProperty("Action");
                if (actionProp?.GetValue(obj)?.ToString() == "Edit")
                {
                    // Open edit popup
                    var editPopup = new JaugeageEditPopup(jaugeage);
                    var editResult = await this.ShowPopupAsync(editPopup);
                    
                    if (editResult != null)
                    {
                        await _viewModel.LoadHistoryAsync();
                    }
                }
            }
        }
    }

    private async void OnDeleteJaugeageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is JaugeageDto jaugeage)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer le jaugeage '{jaugeage.NumeroJaugeage}'?\n\nToutes les mesures associees seront egalement supprimees.",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                var success = await _jaugeageService.DeleteJaugeageAsync(jaugeage.ID);
                
                if (success)
                {
                    await _viewModel.LoadHistoryAsync();
                    await DisplayAlert("Succes", "Jaugeage supprime avec succes", "OK");
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de supprimer le jaugeage", "OK");
                }
            }
        }
    }

    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is JaugeageDto jaugeage)
        {
            var popup = new JaugeageDetailsPopup(jaugeage);
            var result = await this.ShowPopupAsync(popup);

            // Check if user wants to edit from details popup
            if (result is { } obj)
            {
                var type = obj.GetType();
                var actionProp = type.GetProperty("Action");
                if (actionProp?.GetValue(obj)?.ToString() == "Edit")
                {
                    var editPopup = new JaugeageEditPopup(jaugeage);
                    var editResult = await this.ShowPopupAsync(editPopup);
                    
                    if (editResult != null)
                    {
                        await _viewModel.LoadHistoryAsync();
                    }
                }
            }
        }
    }

    #endregion
}
