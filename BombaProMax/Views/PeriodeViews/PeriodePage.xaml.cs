using BombaProMax.Models;
using BombaProMax.ViewModels;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodePage : ContentPage
{
    private readonly PeriodeViewModel _viewModel;
    private readonly PeriodeService _periodeService;

    public PeriodePage(PeriodeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _periodeService = new PeriodeService();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnAddPeriodeClicked(object sender, EventArgs e)
    {
        var popup = new PeriodeCreatePopup(_viewModel);
        var result = await this.ShowPopupAsync(popup);

        if (result is PeriodeWithDetailsDto periodeWithDetails)
        {
            var saveResult = await _viewModel.CreatePeriodeWithDetailsAsync(periodeWithDetails);
            
            if (!saveResult.IsSuccess)
            {
                // Show error alert with API message
                await DisplayAlert(
                    "Erreur",
                    saveResult.ErrorMessage ?? "Impossible de créer la période",
                    "OK");
            }
        }
    }

    private async void OnEditPeriodeClicked(object sender, EventArgs e)
    {
        PeriodeDto? periode = null;

        if (sender is Button button && button.CommandParameter is PeriodeDto p)
        {
            periode = p;
        }
        else
        {
            periode = _viewModel.SelectedPeriode;
        }

        if (periode == null) return;

        try
        {
            // Load the periode details directly from service to ensure we have correct data
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Loading details for edit: Periode {periode.PeriodeID}");
            var existingDetails = await _periodeService.GetDetailsByPeriodeAsync(periode.PeriodeID);
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Loaded {existingDetails.Count} details for edit");

            var popup = new PeriodeEditPopup(periode, existingDetails, _viewModel);
            var result = await this.ShowPopupAsync(popup);

            if (result is PeriodeWithDetailsDto periodeWithDetails)
            {
                var saveResult = await _viewModel.UpdatePeriodeWithDetailsAsync(periodeWithDetails);
                
                if (!saveResult.IsSuccess)
                {
                    // Show error alert with API message
                    await DisplayAlert(
                        "Erreur",
                        saveResult.ErrorMessage ?? "Impossible de mettre à jour la période",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Error loading details for edit: {ex.Message}");
            await DisplayAlert("Erreur", $"Impossible de charger les détails: {ex.Message}", "OK");
        }
    }

    private async void OnViewPeriodeClicked(object sender, EventArgs e)
    {
        PeriodeDto? periode = null;

        if (sender is Button button && button.CommandParameter is PeriodeDto p)
        {
            periode = p;
        }

        if (periode == null) return;

        try
        {
            // Load the periode details directly from service instead of relying on ViewModel
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Loading details for view: Periode {periode.PeriodeID}");
            var details = await _periodeService.GetDetailsByPeriodeAsync(periode.PeriodeID);
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Loaded {details.Count} details for view");
            
            // Show the analytics popup with loaded details
            var popup = new PeriodeViewPopup(periode, details);
            var result = await this.ShowPopupAsync(popup);
            
            // Handle result if edit was requested
            if (result is PeriodeViewResult viewResult && viewResult.Action == "Edit")
            {
                System.Diagnostics.Debug.WriteLine($"[PeriodePage] Edit requested from view popup with {viewResult.Details?.Count ?? 0} details");
                
                // Open edit popup with the details from view result
                var editPopup = new PeriodeEditPopup(viewResult.Periode!, viewResult.Details!, _viewModel);
                var editResult = await this.ShowPopupAsync(editPopup);

                if (editResult is PeriodeWithDetailsDto periodeWithDetails)
                {
                    var saveResult = await _viewModel.UpdatePeriodeWithDetailsAsync(periodeWithDetails);
                    
                    if (!saveResult.IsSuccess)
                    {
                        // Show error alert with API message
                        await DisplayAlert(
                            "Erreur",
                            saveResult.ErrorMessage ?? "Impossible de mettre à jour la période",
                            "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PeriodePage] Error: {ex.Message}");
            await DisplayAlert("Erreur", $"Impossible de charger les détails: {ex.Message}", "OK");
        }
    }

    private async void OnDeletePeriodeClicked(object sender, EventArgs e)
    {
        PeriodeDto? periode = null;

        if (sender is Button button && button.CommandParameter is PeriodeDto p)
        {
            periode = p;
        }
        else
        {
            periode = _viewModel.SelectedPeriode;
        }

        if (periode == null) return;

        bool confirm = await DisplayAlert(
            "Confirmation",
            "Voulez-vous vraiment supprimer cette période et tous ses relevés?",
            "Oui, supprimer",
            "Annuler");

        if (confirm)
        {
            await _viewModel.DeletePeriodeAsync(periode);
        }
    }

    private async void OnAddDetailClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedPeriode == null) return;

        var popup = new PeriodeDetailsPopup(_viewModel.SelectedPeriode.PeriodeID);
        var result = await this.ShowPopupAsync(popup);

        if (result is PeriodeDetailsDto newDetail)
        {
            await _viewModel.CreateDetailAsync(newDetail);
        }
    }

    private async void OnEditDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PeriodeDetailsDto detail)
        {
            var popup = new PeriodeDetailsPopup(_viewModel.SelectedPeriode!.PeriodeID, detail);
            var result = await this.ShowPopupAsync(popup);

            if (result is PeriodeDetailsDto updatedDetail)
            {
                await _viewModel.UpdateDetailAsync(updatedDetail);
            }
        }
    }

    private async void OnDeleteDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PeriodeDetailsDto detail)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer ce relevé de la pompe {detail.PompeNumero}?",
                "Oui, supprimer",
                "Annuler");

            if (confirm)
            {
                await _viewModel.DeleteDetailAsync(detail);
            }
        }
    }
}