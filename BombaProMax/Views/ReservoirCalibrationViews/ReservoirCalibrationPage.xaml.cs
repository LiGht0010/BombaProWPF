using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ReservoirCalibrationViews;

public partial class ReservoirCalibrationPage : ContentPage
{
    private readonly ReservoirCalibrationViewModel _viewModel;

    public ReservoirCalibrationPage(ReservoirCalibrationViewModel viewModel)
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

    private async void OnImportClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedReservoir == null)
        {
            await DisplayAlert("Attention", "Veuillez d'abord sélectionner un réservoir", "OK");
            return;
        }

        var popup = new CalibrationImportPopup(_viewModel.SelectedReservoir);
        var result = await this.ShowPopupAsync(popup);

        if (result is string csvContent && !string.IsNullOrWhiteSpace(csvContent))
        {
            var success = await _viewModel.ImportFromCsvAsync(csvContent);
            if (success)
            {
                await DisplayAlert("Succès", 
                    $"Importation réussie!\n{_viewModel.TotalCalibrations} entrées importées.\nHauteur max: {_viewModel.MaxHauteur:N1} cm", 
                    "OK");
            }
            else
            {
                await DisplayAlert("Erreur", 
                    _viewModel.ErrorMessage ?? "Échec de l'importation", 
                    "OK");
            }
        }
    }

    private async void OnDeleteAllClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedReservoir == null) return;

        var confirm = await DisplayAlert(
            "Confirmer la suppression",
            $"Êtes-vous sûr de vouloir supprimer toutes les données de calibration pour le réservoir '{_viewModel.SelectedReservoir.Numero}'?\n\nCette action est irréversible.",
            "Supprimer",
            "Annuler");

        if (confirm)
        {
            await _viewModel.DeleteAllCalibrationsAsync();
            
            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Succès", "Données de calibration supprimées", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", _viewModel.ErrorMessage, "OK");
            }
        }
    }

    private async void OnTestLookupClicked(object sender, EventArgs e)
    {
        if (!decimal.TryParse(TestHauteurEntry.Text, out var hauteur))
        {
            TestResultLabel.Text = "?? Invalide";
            TestResultLabel.TextColor = Colors.Red;
            return;
        }

        var result = await _viewModel.LookupVolumeAsync(hauteur);
        
        if (result != null)
        {
            TestResultLabel.Text = $"{result.VolumeLitres:N0} L";
            TestResultLabel.TextColor = result.IsInterpolated 
                ? Color.FromArgb("#F57C00")  // Orange for interpolated
                : Color.FromArgb("#00796B"); // Teal for exact match
            
            var interpolationNote = result.IsInterpolated ? " (interpolé)" : " (exact)";
            await DisplayAlert("Résultat", 
                $"Hauteur: {result.HauteurCm:N1} cm\nVolume: {result.VolumeLitres:N2} L{interpolationNote}", 
                "OK");
        }
        else
        {
            TestResultLabel.Text = "? Non trouvé";
            TestResultLabel.TextColor = Colors.Red;
        }
    }
}