using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeageDetailsPopup : Popup
{
    private readonly JaugeageDto _jaugeageDto;
    private readonly JaugeageService _jaugeageService;
    private readonly UserService _userService;
    private readonly StockLotService _stockLotService;
    private JaugeageWithDetailsDto? _jaugeageWithDetails;

    public JaugeageDetailsPopup(JaugeageDto jaugeage)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _jaugeageDto = jaugeage;
        _jaugeageService = new JaugeageService();
        _userService = new UserService();
        _stockLotService = new StockLotService();

        // Set initial header info
        NumeroLabel.Text = jaugeage.NumeroJaugeage ?? "-";
        DateLabel.Text = jaugeage.DateJaugeage.ToString("dd/MM/yyyy");
        
        // Load full details
        _ = LoadDetailsAsync();
    }

    private async Task LoadDetailsAsync()
    {
        try
        {
            _jaugeageWithDetails = await _jaugeageService.GetJaugeageWithDetailsAsync(_jaugeageDto.ID);
            
            if (_jaugeageWithDetails == null)
            {
                Debug.WriteLine($"[JaugeageDetailsPopup] Could not load details for ID {_jaugeageDto.ID}");
                return;
            }

            // Update summary
            ReservoirsCountLabel.Text = _jaugeageWithDetails.DetailsCount.ToString();
            TotalVolumeLabel.Text = $"{_jaugeageWithDetails.TotalVolume:N0} L";
            TemoinLabel.Text = _jaugeageWithDetails.TemoinNom ?? "-";

            // Populate details rows
            DetailsContainer.Children.Clear();
            
            foreach (var detail in _jaugeageWithDetails.Details)
            {
                DetailsContainer.Children.Add(CreateDetailRow(detail));
            }

            // Show empty message if no details
            if (_jaugeageWithDetails.Details.Count == 0)
            {
                DetailsContainer.Children.Add(new Label
                {
                    Text = "Aucune mesure enregistree",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#9E9E9E"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
            }

            // Observations
            if (!string.IsNullOrWhiteSpace(_jaugeageWithDetails.Observations))
            {
                ObservationsCard.IsVisible = true;
                ObservationsLabel.Text = _jaugeageWithDetails.Observations;
            }

            // Audit info - dates
            DateCreationLabel.Text = _jaugeageWithDetails.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            DateModificationLabel.Text = _jaugeageWithDetails.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "-";

            // Load user names asynchronously
            await LoadAuditUserNamesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageDetailsPopup] Error loading details: {ex.Message}");
        }
    }

    private async Task LoadAuditUserNamesAsync()
    {
        try
        {
            if (_jaugeageWithDetails == null) return;

            // Load created by user name
            var createdByName = await _userService.GetUserNameByIdAsync(_jaugeageWithDetails.AjoutePar);
            CreeParLabel.Text = createdByName;

            // Load modified by user name
            var modifiedByName = await _userService.GetUserNameByIdAsync(_jaugeageWithDetails.ModifiePar);
            ModifieParLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageDetailsPopup] Error loading audit info: {ex.Message}");
            CreeParLabel.Text = "Erreur de chargement";
            ModifieParLabel.Text = "Erreur de chargement";
        }
    }

    private View CreateDetailRow(JaugeageDetailDto detail)
    {
        var container = new VerticalStackLayout { Spacing = 0 };

        var grid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) }
            ],
            ColumnSpacing = 8,
            Padding = new Thickness(12, 10),
            BackgroundColor = Colors.Transparent
        };

        // Reservoir numero badge
        var numeroBorder = new Border
        {
            BackgroundColor = Color.FromArgb("#E0F2F1"),
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = detail.ReservoirNumero ?? $"#{detail.ReservoirID}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00796B"),
                VerticalTextAlignment = TextAlignment.Center
            }
        };
        grid.Add(numeroBorder, 0);

        // Produit (we don't have this in detail, show placeholder)
        grid.Add(new Label
        {
            Text = "-",
            FontSize = 12,
            TextColor = Color.FromArgb("#666"),
            VerticalTextAlignment = TextAlignment.Center
        }, 1);

        // Hauteur
        grid.Add(new Label
        {
            Text = $"{detail.HauteurMesuree:N1} cm",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1976D2"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        }, 2);

        // Volume badge
        var volumeBorder = new Border
        {
            BackgroundColor = Color.FromArgb("#E8F5E9"),
            Padding = new Thickness(10, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            HorizontalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = $"{detail.VolumeCalcule:N0} L",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2E7D32"),
                VerticalTextAlignment = TextAlignment.Center
            }
        };
        grid.Add(volumeBorder, 3);

        // Temperature
        grid.Add(new Label
        {
            Text = detail.Temperature.HasValue ? $"{detail.Temperature:N1}°C" : "-",
            FontSize = 12,
            TextColor = Color.FromArgb("#666"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        }, 4);

        container.Add(grid);
        container.Add(new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(12, 0)
        });

        return container;
    }

    private async void OnCalibrateClicked(object sender, EventArgs e)
    {
        try
        {
            CalibrateButton.IsEnabled = false;
            CalibrateButton.Text = "? Chargement...";

            // Get calibration preview first
            var preview = await _stockLotService.GetCalibrationPreviewAsync(_jaugeageDto.ID);
            
            if (preview == null)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur",
                    "Impossible de charger la prévisualisation de calibration.",
                    "OK");
                return;
            }

            // Check if any adjustments needed
            if (preview.ReservoirsNeedingAdjustment == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Stock déjà calibré",
                    "Tous les réservoirs sont déjà alignés avec ce jaugeage. Aucun ajustement nécessaire.",
                    "OK");
                return;
            }

            // Build preview message
            var previewMessage = BuildCalibrationPreviewMessage(preview);

            // Confirm with user
            var confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Confirmer la calibration",
                previewMessage,
                "Oui, calibrer",
                "Annuler");

            if (!confirmed)
                return;

            CalibrateButton.Text = "? Calibration...";

            // Perform calibration
            var request = new StockCalibrationRequestDto
            {
                JaugeageId = _jaugeageDto.ID,
                UtilisateurCalibration = App.CurrentUser?.Name ?? App.user?.Name,
                Notes = $"Calibration manuelle depuis le détail du jaugeage {_jaugeageDto.NumeroJaugeage}"
            };

            var (success, result, errorMessage) = await _stockLotService.CalibrateToJaugeageAsync(request);

            if (success && result != null)
            {
                // Build success message with details
                var successMessage = BuildCalibrationResultMessage(result);

                await Application.Current!.MainPage!.DisplayAlert(
                    "? Calibration réussie",
                    successMessage,
                    "OK");

                // Close popup and signal refresh needed
                Close(new { Action = "Calibrated", JaugeageId = _jaugeageDto.ID, Result = result });
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur de calibration",
                    errorMessage ?? result?.Message ?? "Une erreur s'est produite lors de la calibration.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageDetailsPopup] Calibration error: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur",
                $"Erreur lors de la calibration: {ex.Message}",
                "OK");
        }
        finally
        {
            CalibrateButton.IsEnabled = true;
            CalibrateButton.Text = "?? Calibrer à ce jaugeage";
        }
    }

    /// <summary>
    /// Builds a user-friendly preview message for calibration confirmation.
    /// </summary>
    private static string BuildCalibrationPreviewMessage(StockCalibrationPreviewDto preview)
    {
        var lines = new List<string>
        {
            $"Jaugeage: {preview.JaugeageNumero}",
            $"Date: {preview.DateJaugeage:dd/MM/yyyy}",
            "",
            $"?? {preview.ReservoirsNeedingAdjustment} réservoir(s) nécessite(nt) un ajustement:",
            ""
        };

        foreach (var res in preview.Reservoirs.Where(r => Math.Abs(r.Difference) > 0.01m))
        {
            var icon = res.Difference > 0 ? "?" : "?";
            var action = res.Difference > 0 ? "Ajouter" : "Réduire";
            lines.Add($"{icon} {res.ReservoirNumero}: {action} {Math.Abs(res.Difference):N0}L");
            lines.Add($"   (Jaugeage: {res.VolumeJaugeage:N0}L, Système: {res.StockSysteme:N0}L)");
            
            if (!res.CanReduce && res.Difference < 0)
            {
                lines.Add($"   ?? {res.WarningMessage}");
            }
        }

        lines.Add("");
        lines.Add("Voulez-vous appliquer ces ajustements?");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Builds a user-friendly result message after calibration.
    /// </summary>
    private static string BuildCalibrationResultMessage(StockCalibrationResultDto result)
    {
        var lines = new List<string>
        {
            "La calibration a été effectuée avec succès!",
            ""
        };

        if (result.TotalStockAdded > 0)
        {
            lines.Add($"? Stock ajouté: {result.TotalStockAdded:N0}L");
            lines.Add($"   ({result.AdjustmentLotsCreated} lot(s) d'ajustement créé(s))");
        }

        if (result.TotalStockReduced > 0)
        {
            lines.Add($"? Stock réduit: {result.TotalStockReduced:N0}L");
        }

        lines.Add("");
        lines.Add("Détails par réservoir:");

        foreach (var res in result.Reservoirs.Where(r => r.ActionPerformed != "Aucun"))
        {
            var icon = res.ActionPerformed == "Ajout" ? "?" : "??";
            lines.Add($"{icon} {res.ReservoirNumero}: {res.StockBefore:N0}L ? {res.StockAfter:N0}L");
        }

        return string.Join("\n", lines);
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        // Close and return a signal to open edit popup
        Close(new { Action = "Edit", Jaugeage = _jaugeageDto });
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}