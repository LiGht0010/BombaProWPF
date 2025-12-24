using BombaProMax.Models;
using BombaProMax.ViewModels;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodePage : ContentPage
{
    private readonly PeriodeViewModel _viewModel;
    private Border? _selectedBorder;

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

    private void OnPeriodeItemTapped(object? sender, TappedEventArgs e)
    {
        // Reset previous selection
        if (_selectedBorder != null)
        {
            _selectedBorder.BackgroundColor = Colors.Transparent;
            _selectedBorder.Stroke = Colors.Transparent;
            _selectedBorder.StrokeThickness = 0;
        }

        // Highlight new selection
        if (sender is Border border)
        {
            border.BackgroundColor = Color.FromArgb("#E3F2FD");
            border.Stroke = Color.FromArgb("#1976D2");
            border.StrokeThickness = 2;
            _selectedBorder = border;
        }
    }

    private async void OnAddPeriodeClicked(object sender, EventArgs e)
    {
        var popup = new PeriodeCreatePopup();
        var result = await this.ShowPopupAsync(popup);

        // Handle the new PeriodeWithDetailsDto result
        if (result is PeriodeWithDetailsDto periodeWithDetails)
        {
            await _viewModel.CreatePeriodeWithDetailsAsync(periodeWithDetails);
        }
    }

    private async void OnEditPeriodeClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedPeriode == null) return;

        // Get existing details for this periode
        var existingDetails = _viewModel.CurrentPeriodeDetails.ToList();

        var popup = new PeriodeEditPopup(_viewModel.SelectedPeriode, existingDetails);
        var result = await this.ShowPopupAsync(popup);

        if (result is PeriodeWithDetailsDto periodeWithDetails)
        {
            await _viewModel.UpdatePeriodeWithDetailsAsync(periodeWithDetails);
        }
    }

    private async void OnDeletePeriodeClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedPeriode == null) return;

        bool confirm = await DisplayAlert(
            "Confirmation",
            "Voulez-vous vraiment supprimer cette période et tous ses relevés?",
            "Oui, supprimer",
            "Annuler");

        if (confirm)
        {
            await _viewModel.DeletePeriodeAsync(_viewModel.SelectedPeriode);
            // Clear selection visual
            if (_selectedBorder != null)
            {
                _selectedBorder.BackgroundColor = Colors.Transparent;
                _selectedBorder.Stroke = Colors.Transparent;
                _selectedBorder.StrokeThickness = 0;
                _selectedBorder = null;
            }
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