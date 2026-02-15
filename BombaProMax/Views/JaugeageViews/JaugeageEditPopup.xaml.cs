using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeageEditPopup : Popup
{
    private readonly JaugeageDto _jaugeageDto;
    private readonly JaugeageService _jaugeageService;
    private readonly ReservoirService _reservoirService;
    private readonly EmployeService _employeService;
    private readonly JaugeageDetailService _detailService;
    
    private JaugeageWithDetailsDto? _jaugeageWithDetails;
    private List<EmployeDto> _employes = [];
    private List<EditReservoirMeasurementRow> _reservoirRows = [];

    public JaugeageEditPopup(JaugeageDto jaugeage)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _jaugeageDto = jaugeage;
        
        _jaugeageService = new JaugeageService();
        _reservoirService = new ReservoirService();
        _employeService = new EmployeService();
        _detailService = new JaugeageDetailService();
        
        // Set header info
        HeaderLabel.Text = $"Modifier: {jaugeage.NumeroJaugeage}";
        SubHeaderLabel.Text = $"Date: {jaugeage.DateJaugeage:dd/MM/yyyy}";
        
        // Load data
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load the full jaugeage with details
            _jaugeageWithDetails = await _jaugeageService.GetJaugeageWithDetailsAsync(_jaugeageDto.ID);
            
            if (_jaugeageWithDetails == null)
            {
                ShowError("Impossible de charger les details du jaugeage");
                return;
            }

            // Load employees for temoin picker
            _employes = await _employeService.GetAllEmployesAsync();
            TemoinPicker.ItemsSource = _employes.Select(e => $"{e.Nom} {e.Prenom}").ToList();
            
            // Set current temoin
            var temoinIndex = _employes.FindIndex(e => e.ID == _jaugeageWithDetails.TemoinID);
            if (temoinIndex >= 0)
            {
                TemoinPicker.SelectedIndex = temoinIndex;
            }

            // Set date (convert UTC to local for display)
            JaugeageDatePicker.Date = _jaugeageWithDetails.DateJaugeage.Date;
            JaugeageDatePicker.MaximumDate = DateTime.Today;
            
            // Set other fields
            NumeroEntry.Text = _jaugeageWithDetails.NumeroJaugeage;
            ObservationsEntry.Text = _jaugeageWithDetails.Observations;
            
            // Load all reservoirs
            var allReservoirs = await _reservoirService.GetAllReservoirsAsync();
            
            ReservoirsContainer.Children.Clear();
            _reservoirRows.Clear();
            
            foreach (var reservoir in allReservoirs)
            {
                // Find if this reservoir has existing measurement
                var existingDetail = _jaugeageWithDetails.Details
                    .FirstOrDefault(d => d.ReservoirID == reservoir.ID);
                
                var row = new EditReservoirMeasurementRow(reservoir, _detailService, OnMeasurementChanged, existingDetail);
                _reservoirRows.Add(row);
                ReservoirsContainer.Children.Add(row.CreateView());
            }
            
            UpdateSummary();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageEditPopup] Error loading data: {ex.Message}");
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
            
            // Update the jaugeage basic info
            var updatedJaugeage = new JaugeageDto
            {
                ID = _jaugeageDto.ID,
                DateJaugeage = DateTime.SpecifyKind(JaugeageDatePicker.Date, DateTimeKind.Utc),
                TemoinID = selectedTemoin.ID,
                NumeroJaugeage = _jaugeageDto.NumeroJaugeage, // Keep original numero
                Observations = string.IsNullOrWhiteSpace(ObservationsEntry.Text) ? null : ObservationsEntry.Text
            };

            var success = await _jaugeageService.UpdateJaugeageAsync(updatedJaugeage);

            if (success)
            {
                // Now update the details
                // First, delete existing details and recreate them
                // This is simpler than trying to track individual changes
                await UpdateJaugeageDetailsAsync();
                
                Close(true); // Return true to indicate success
            }
            else
            {
                ShowError("Erreur lors de la mise a jour");
                SaveButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageEditPopup] Save error: {ex.Message}");
            ShowError($"Erreur: {ex.Message}");
            SaveButton.IsEnabled = true;
        }
    }

    private async Task UpdateJaugeageDetailsAsync()
    {
        // Get the new measurements
        var newDetails = _reservoirRows
            .Where(r => r.HauteurMesuree > 0)
            .Select(r => new JaugeageDetailDto
            {
                JaugeageID = _jaugeageDto.ID,
                ReservoirID = r.ReservoirID,
                HauteurMesuree = r.HauteurMesuree,
                VolumeCalcule = r.VolumeCalcule,
                Temperature = null,
                Notes = null
            })
            .ToList();

        // Delete existing details that are not in new list or have changed
        if (_jaugeageWithDetails?.Details != null)
        {
            foreach (var existingDetail in _jaugeageWithDetails.Details)
            {
                var matchingNew = newDetails.FirstOrDefault(n => n.ReservoirID == existingDetail.ReservoirID);
                
                if (matchingNew == null || 
                    matchingNew.HauteurMesuree != existingDetail.HauteurMesuree)
                {
                    // Delete this detail
                    await _detailService.DeleteDetailAsync(existingDetail.ID);
                }
                else
                {
                    // Remove from new list since it already exists and hasn't changed
                    newDetails.Remove(matchingNew);
                }
            }
        }

        // Add new/changed details
        foreach (var detail in newDetails)
        {
            await _detailService.CreateDetailAsync(detail);
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
/// Helper class to manage a single reservoir measurement row for editing
/// </summary>
public class EditReservoirMeasurementRow
{
    private readonly ReservoirDto _reservoir;
    private readonly JaugeageDetailService _detailService;
    private readonly Action _onChanged;
    private readonly JaugeageDetailDto? _existingDetail;
    
    private Entry? _hauteurEntry;
    private Label? _volumeLabel;
    private Label? _statusLabel;
    private System.Timers.Timer? _debounceTimer;

    public int ReservoirID => _reservoir.ID;
    public decimal HauteurMesuree { get; private set; }
    public decimal VolumeCalcule { get; private set; }

    public EditReservoirMeasurementRow(
        ReservoirDto reservoir, 
        JaugeageDetailService detailService, 
        Action onChanged,
        JaugeageDetailDto? existingDetail = null)
    {
        _reservoir = reservoir;
        _detailService = detailService;
        _onChanged = onChanged;
        _existingDetail = existingDetail;
        
        // Initialize with existing values if present
        if (existingDetail != null)
        {
            HauteurMesuree = existingDetail.HauteurMesuree;
            VolumeCalcule = existingDetail.VolumeCalcule;
        }
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
            BackgroundColor = _existingDetail != null 
                ? Color.FromArgb("#F3F8FF") // Light blue for existing measurements
                : Color.FromArgb("#FAFAFA")
        };

        // Reservoir numero
        var numeroBorder = new Border
        {
            BackgroundColor = _existingDetail != null 
                ? Color.FromArgb("#E3F2FD") 
                : Color.FromArgb("#E0F2F1"),
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = _reservoir.Numero,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = _existingDetail != null 
                    ? Color.FromArgb("#1976D2") 
                    : Color.FromArgb("#00796B"),
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

        // Hauteur Entry - pre-populate with existing value
        _hauteurEntry = new Entry
        {
            Placeholder = "0",
            Keyboard = Keyboard.Numeric,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            BackgroundColor = Colors.White,
            Text = _existingDetail != null ? _existingDetail.HauteurMesuree.ToString("0.##") : ""
        };
        _hauteurEntry.TextChanged += OnHauteurChanged;
        
        var hauteurBorder = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = _existingDetail != null 
                ? Color.FromArgb("#1976D2") 
                : Color.FromArgb("#E0E0E0"),
            StrokeThickness = _existingDetail != null ? 2 : 1,
            Padding = new Thickness(4),
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Content = _hauteurEntry
        };
        grid.Add(hauteurBorder, 2);

        // Volume Label - pre-populate with existing value
        _volumeLabel = new Label
        {
            Text = _existingDetail != null ? $"{_existingDetail.VolumeCalcule:N0}" : "0",
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
            Text = _existingDetail != null ? "Existant" : "-",
            FontSize = 11,
            TextColor = _existingDetail != null 
                ? Color.FromArgb("#1976D2") 
                : Color.FromArgb("#9E9E9E"),
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
                
                // Check if value changed from original
                if (_existingDetail != null && hauteur == _existingDetail.HauteurMesuree)
                {
                    _statusLabel.Text = "Existant";
                    _statusLabel.TextColor = Color.FromArgb("#1976D2");
                }
                else
                {
                    _statusLabel.Text = "Modifie";
                    _statusLabel.TextColor = Color.FromArgb("#FF9800");
                }
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
            Debug.WriteLine($"[EditReservoirRow] Calc error: {ex.Message}");
            VolumeCalcule = 0;
            _volumeLabel!.Text = "Err";
            _statusLabel.Text = "Erreur";
            _statusLabel.TextColor = Color.FromArgb("#F44336");
        }

        _onChanged?.Invoke();
    }
}