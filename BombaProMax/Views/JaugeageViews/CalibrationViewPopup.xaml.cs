using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.JaugeageViews;

public partial class CalibrationViewPopup : Popup
{
    private readonly ReservoirDto _reservoir;
    private readonly JaugeageViewModel _viewModel;
    private List<ReservoirCalibrationDto> _calibrations = [];

    public CalibrationViewPopup(ReservoirDto reservoir, JaugeageViewModel viewModel)
    {
        InitializeComponent();
        _reservoir = reservoir;
        _viewModel = viewModel;
        
        HeaderLabel.Text = $"Calibration: {reservoir.Numero}";
        SubHeaderLabel.Text = $"{reservoir.ProduitNom ?? "Non assigne"} - Capacite: {reservoir.Capacite:N0} L";
        
        _ = LoadCalibrationDataAsync();
    }

    private async Task LoadCalibrationDataAsync()
    {
        try
        {
            // Load calibrations for this reservoir
            await _viewModel.LoadCalibrationsAsync(_reservoir.ID);
            _calibrations = _viewModel.Calibrations.ToList();
            
            // Update stats
            TotalEntriesLabel.Text = _calibrations.Count.ToString();
            
            if (_calibrations.Count > 0)
            {
                MinHauteurLabel.Text = $"{_calibrations.Min(c => c.HauteurCm):N1} cm";
                MaxHauteurLabel.Text = $"{_calibrations.Max(c => c.HauteurCm):N1} cm";
                MaxVolumeLabel.Text = $"{_calibrations.Max(c => c.VolumeLitres):N0} L";
            }
            else
            {
                MinHauteurLabel.Text = "-- cm";
                MaxHauteurLabel.Text = "-- cm";
                MaxVolumeLabel.Text = "-- L";
            }
            
            // Build table rows
            BuildCalibrationRows();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CalibrationViewPopup] Error loading data: {ex.Message}");
        }
    }

    private void BuildCalibrationRows()
    {
        CalibrationContainer.Children.Clear();
        
        int index = 1;
        foreach (var calibration in _calibrations.OrderBy(c => c.HauteurCm))
        {
            var isEven = index % 2 == 0;
            
            var grid = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                ],
                ColumnSpacing = 8,
                Padding = new Thickness(12, 8),
                BackgroundColor = isEven ? Color.FromArgb("#FAFAFA") : Colors.White
            };
            
            grid.Add(new Label
            {
                Text = index.ToString(),
                FontSize = 11,
                TextColor = Color.FromArgb("#8B939E"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }, 0);
            
            grid.Add(new Label
            {
                Text = calibration.HauteurCm.ToString("N1"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00796B"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }, 1);
            
            grid.Add(new Label
            {
                Text = calibration.VolumeLitres.ToString("N0"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1976D2"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }, 2);
            
            CalibrationContainer.Children.Add(grid);
            
            // Add divider
            CalibrationContainer.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#E0E0E0"),
                Margin = new Thickness(0)
            });
            
            index++;
        }
        
        if (_calibrations.Count == 0)
        {
            CalibrationContainer.Children.Add(new Label
            {
                Text = "Aucune donnee de calibration",
                FontSize = 14,
                TextColor = Color.FromArgb("#8B939E"),
                HorizontalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 40)
            });
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

        var result = await _viewModel.LookupVolumeForReservoirAsync(_reservoir.ID, hauteur);
        
        if (result != null)
        {
            TestResultLabel.Text = $"{result.VolumeLitres:N0} L";
            TestResultLabel.TextColor = result.IsInterpolated 
                ? Color.FromArgb("#F57C00")  // Orange for interpolated
                : Color.FromArgb("#00796B"); // Teal for exact match
        }
        else
        {
            TestResultLabel.Text = "? Non trouve";
            TestResultLabel.TextColor = Colors.Red;
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
