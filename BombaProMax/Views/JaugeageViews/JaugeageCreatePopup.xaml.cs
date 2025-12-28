using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeageCreatePopup : Popup
{
    private readonly JaugeageService _jaugeageService;
    private readonly ReservoirService _reservoirService;
    private readonly EmployeService _employeService;
    private readonly JaugeageDetailService _detailService;
    
    private List<EmployeDto> _employes = [];
    private List<ReservoirMeasurementRow> _reservoirRows = [];

    public JaugeageCreatePopup()
    {
        InitializeComponent();
        
        _jaugeageService = new JaugeageService();
        _reservoirService = new ReservoirService();
        _employeService = new EmployeService();
        _detailService = new JaugeageDetailService();
        
        JaugeageDatePicker.Date = DateTime.Today;
        JaugeageDatePicker.MaximumDate = DateTime.Today;
        
        // Load data
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load employees for temoin picker
            _employes = await _employeService.GetAllEmployesAsync();
            TemoinPicker.ItemsSource = _employes.Select(e => $"{e.Nom} {e.Prenom}").ToList();
            
            // Load reservoirs
            var reservoirs = await _reservoirService.GetAllReservoirsAsync();
            
            ReservoirsContainer.Children.Clear();
            _reservoirRows.Clear();
            
            foreach (var reservoir in reservoirs)
            {
                var row = new ReservoirMeasurementRow(reservoir, _detailService, OnMeasurementChanged);
                _reservoirRows.Add(row);
                ReservoirsContainer.Children.Add(row.CreateView());
            }
            
            UpdateSummary();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageCreatePopup] Error loading data: {ex.Message}");
            ShowError("Erreur lors du chargement des donnees");
        }
    }

    private void OnMeasurementChanged()
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var totalReservoirs = _reservoirRows.Count;
        var measuredCount = _reservoirRows.Count(r => r.HauteurMesuree > 0);
        var totalVolume = _reservoirRows.Sum(r => r.VolumeCalcule);
        
        TotalReservoirsLabel.Text = totalReservoirs.ToString();
        MeasuredCountLabel.Text = measuredCount.ToString();
        TotalVolumeLabel.Text = $"{totalVolume:N0} L";
        SummaryVolumeLabel.Text = $"{totalVolume:N0} L";
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        try
        {
            SaveButton.IsEnabled = false;
            ErrorLabel.IsVisible = false;

            var selectedTemoin = _employes[TemoinPicker.SelectedIndex];
            
            // Build the jaugeage with details
            var jaugeage = new JaugeageWithDetailsDto
            {
                DateJaugeage = DateTime.SpecifyKind(JaugeageDatePicker.Date, DateTimeKind.Utc),
                TemoinID = selectedTemoin.ID,
                NumeroJaugeage = string.IsNullOrWhiteSpace(NumeroEntry.Text) ? null : NumeroEntry.Text,
                Observations = string.IsNullOrWhiteSpace(ObservationsEntry.Text) ? null : ObservationsEntry.Text,
                Details = _reservoirRows
                    .Where(r => r.HauteurMesuree > 0)
                    .Select(r => new JaugeageDetailDto
                    {
                        ReservoirID = r.ReservoirID,
                        HauteurMesuree = r.HauteurMesuree,
                        VolumeCalcule = r.VolumeCalcule,
                        Temperature = null,
                        Notes = null
                    })
                    .ToList()
            };

            var result = await _jaugeageService.CreateJaugeageWithDetailsAsync(jaugeage);

            if (result != null)
            {
                Close(result);
            }
            else
            {
                ShowError("Erreur lors de l'enregistrement");
                SaveButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageCreatePopup] Save error: {ex.Message}");
            ShowError($"Erreur: {ex.Message}");
            SaveButton.IsEnabled = true;
        }
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        if (TemoinPicker.SelectedIndex < 0)
        {
            ShowError("Veuillez selectionner un temoin");
            return false;
        }

        var measuredCount = _reservoirRows.Count(r => r.HauteurMesuree > 0);
        if (measuredCount == 0)
        {
            ShowError("Veuillez mesurer au moins un reservoir");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}

/// <summary>
/// Helper class to manage a single reservoir measurement row
/// </summary>
public class ReservoirMeasurementRow
{
    private readonly ReservoirDto _reservoir;
    private readonly JaugeageDetailService _detailService;
    private readonly Action _onChanged;
    
    private Entry? _hauteurEntry;
    private Label? _volumeLabel;
    private Label? _statusLabel;
    private System.Timers.Timer? _debounceTimer;

    public int ReservoirID => _reservoir.ID;
    public decimal HauteurMesuree { get; private set; }
    public decimal VolumeCalcule { get; private set; }

    public ReservoirMeasurementRow(ReservoirDto reservoir, JaugeageDetailService detailService, Action onChanged)
    {
        _reservoir = reservoir;
        _detailService = detailService;
        _onChanged = onChanged;
    }

    public View CreateView()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) }
            ],
            ColumnSpacing = 8,
            Padding = new Thickness(12, 8),
            BackgroundColor = Color.FromArgb("#FAFAFA")
        };

        // Reservoir numero
        var numeroBorder = new Border
        {
            BackgroundColor = Color.FromArgb("#E0F2F1"),
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = _reservoir.Numero,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#00796B"),
                VerticalTextAlignment = TextAlignment.Center
            }
        };
        grid.Add(numeroBorder, 0);

        // Produit
        grid.Add(new Label
        {
            Text = _reservoir.ProduitNom ?? "-",
            FontSize = 12,
            TextColor = Color.FromArgb("#333"),
            VerticalTextAlignment = TextAlignment.Center
        }, 1);

        // Hauteur Entry
        _hauteurEntry = new Entry
        {
            Placeholder = "0",
            Keyboard = Keyboard.Numeric,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            BackgroundColor = Colors.White
        };
        _hauteurEntry.TextChanged += OnHauteurChanged;
        
        var hauteurBorder = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E0E0E0"),
            StrokeThickness = 1,
            Padding = new Thickness(4),
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Content = _hauteurEntry
        };
        grid.Add(hauteurBorder, 2);

        // Volume Label
        _volumeLabel = new Label
        {
            Text = "0",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E7D32"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(_volumeLabel, 3);

        // Status Label
        _statusLabel = new Label
        {
            Text = "-",
            FontSize = 11,
            TextColor = Color.FromArgb("#9E9E9E"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(_statusLabel, 4);

        // Add bottom border
        var container = new VerticalStackLayout { Spacing = 0 };
        container.Add(grid);
        container.Add(new BoxView 
        { 
            HeightRequest = 1, 
            Color = Color.FromArgb("#E0E0E0"),
            Margin = new Thickness(12, 0)
        });

        return container;
    }

    private void OnHauteurChanged(object? sender, TextChangedEventArgs e)
    {
        // Debounce the calculation
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Timers.Timer(300);
        _debounceTimer.Elapsed += async (s, args) =>
        {
            _debounceTimer?.Stop();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await CalculateVolumeAsync();
            });
        };
        _debounceTimer.Start();
    }

    private async Task CalculateVolumeAsync()
    {
        if (!decimal.TryParse(_hauteurEntry?.Text, out var hauteur) || hauteur <= 0)
        {
            HauteurMesuree = 0;
            VolumeCalcule = 0;
            _volumeLabel!.Text = "0";
            _statusLabel!.Text = "-";
            _statusLabel.TextColor = Color.FromArgb("#9E9E9E");
            _onChanged?.Invoke();
            return;
        }

        HauteurMesuree = hauteur;
        _statusLabel!.Text = "...";
        _statusLabel.TextColor = Color.FromArgb("#FF9800");

        try
        {
            var result = await _detailService.CalculateVolumeAsync(_reservoir.ID, hauteur);
            
            if (result != null)
            {
                VolumeCalcule = result.VolumeLitres;
                _volumeLabel!.Text = $"{result.VolumeLitres:N0}";
                _statusLabel.Text = result.IsInterpolated ? "Interpole" : "Exact";
                _statusLabel.TextColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                VolumeCalcule = 0;
                _volumeLabel!.Text = "?";
                _statusLabel.Text = "Pas de calib.";
                _statusLabel.TextColor = Color.FromArgb("#F44336");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReservoirRow] Calc error: {ex.Message}");
            VolumeCalcule = 0;
            _volumeLabel!.Text = "Err";
            _statusLabel.Text = "Erreur";
            _statusLabel.TextColor = Color.FromArgb("#F44336");
        }

        _onChanged?.Invoke();
    }
}