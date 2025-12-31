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
    private JaugeageWithDetailsDto? _jaugeageWithDetails;

    public JaugeageDetailsPopup(JaugeageDto jaugeage)
    {
        InitializeComponent();
        _jaugeageDto = jaugeage;
        _jaugeageService = new JaugeageService();
        _userService = new UserService();

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