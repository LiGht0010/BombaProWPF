using BombaProMax.Models;
using BombaProMax.ViewModels;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodePage : ContentPage
{
    private readonly PeriodeViewModel _viewModel;

    public PeriodePage(PeriodeViewModel viewModel)
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

    private async void OnAddPeriodeClicked(object sender, EventArgs e)
    {
        var popup = new PeriodeCreatePopup(_viewModel);
        var result = await this.ShowPopupAsync(popup);

        if (result is PeriodeWithDetailsDto periodeWithDetails)
        {
            await _viewModel.CreatePeriodeWithDetailsAsync(periodeWithDetails);
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

        // Select the periode first to load its details
        _viewModel.SelectPeriodeCommand.Execute(periode);
        await Task.Delay(100);

        var existingDetails = _viewModel.CurrentPeriodeDetails.ToList();
        var popup = new PeriodeEditPopup(periode, existingDetails, _viewModel);
        var result = await this.ShowPopupAsync(popup);

        if (result is PeriodeWithDetailsDto periodeWithDetails)
        {
            await _viewModel.UpdatePeriodeWithDetailsAsync(periodeWithDetails);
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

        // Select the periode to load its details
        _viewModel.SelectPeriodeCommand.Execute(periode);
        await Task.Delay(100);
        
        // Build details string
        var details = _viewModel.CurrentPeriodeDetails.ToList();
        var detailsText = details.Count > 0
            ? string.Join("\n", details.Select(d => 
                $"• Pompe {d.PompeNumero}: {d.QuantiteVendue:N2} L ({d.PrixTotal:N2} MAD)"))
            : "Aucun relevé enregistré";
        
        // Show alert with detailed info
        await DisplayAlert(
            "Détails de la Période",
            $"Date: {periode.DateDebut:dd/MM/yyyy HH:mm} - {periode.DateFin:dd/MM/yyyy HH:mm}\n" +
            $"Employé: {periode.EmployeNom ?? "Non assigné"}\n" +
            $"TPE: {periode.TPE:N2} MAD\n" +
            $"Espèces: {periode.Especes:N2} MAD\n\n" +
            $"Relevés ({details.Count}):\n{detailsText}",
            "OK");
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