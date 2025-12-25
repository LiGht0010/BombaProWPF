using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodeCreatePopup : Popup
{
    private readonly PeriodeDto? _existingPeriode;
    private readonly bool _isEditMode;
    private readonly PeriodeViewModel _viewModel;

    public PeriodeCreatePopup(PeriodeViewModel viewModel, PeriodeDto? existingPeriode = null)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _existingPeriode = existingPeriode;
        _isEditMode = existingPeriode != null;
        
        BindingContext = _viewModel;

        // Set default date/time values
        var now = DateTime.Now;
        DateDebutPicker.Date = now.Date;
        TimeDebutPicker.Time = new TimeSpan(now.Hour, 0, 0);
        DateFinPicker.Date = now.Date;
        TimeFinPicker.Time = new TimeSpan(Math.Min(now.Hour + 8, 23), 0, 0);
        
        // Set default payment values
        TPEEntry.Text = "0";
        EspecesEntry.Text = "0";

        if (_isEditMode && existingPeriode != null)
        {
            HeaderLabel.Text = "?? Modifier Période";
            SaveButton.Text = "? Mettre à jour";
            PopulateForEdit(existingPeriode);
        }

        // Load all data
        _ = LoadAllDataAsync();
    }

    private void PopulateForEdit(PeriodeDto periode)
    {
        DateDebutPicker.Date = periode.DateDebut.Date;
        TimeDebutPicker.Time = periode.DateDebut.TimeOfDay;
        DateFinPicker.Date = periode.DateFin.Date;
        TimeFinPicker.Time = periode.DateFin.TimeOfDay;
        
        // Populate TPE and Especes
        TPEEntry.Text = periode.TPE.ToString("F2");
        EspecesEntry.Text = periode.Especes.ToString("F2");
    }

    private async Task LoadAllDataAsync()
    {
        try
        {
            // Load reference data through ViewModel
            await _viewModel.LoadReferenceDataAsync();

            // Set employee picker
            EmployePicker.ItemsSource = _viewModel.Employes.ToList();
            if (_isEditMode && _existingPeriode?.EmployeID != null)
            {
                var employe = _viewModel.Employes.FirstOrDefault(e => e.ID == _existingPeriode.EmployeID);
                if (employe != null)
                    EmployePicker.SelectedItem = employe;
            }

            // Build pump readings list
            _viewModel.BuildPompeReadingsForCreate();
            
            // Bind collection view
            PompesCollectionView.ItemsSource = _viewModel.PompeReadings;

            UpdateSummary();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            ShowError("Erreur de chargement des données");
        }
    }

    private void OnMeterValueChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateSummary();
    }
    
    private void OnPaymentValueChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        decimal totalQuantite = 0;
        decimal totalRecette = 0;

        foreach (var reading in _viewModel.PompeReadings)
        {
            totalQuantite += reading.QuantiteVendue;
            totalRecette += reading.PrixTotal;
        }

        TotalQuantiteLabel.Text = $"{totalQuantite:N2} L";
        TotalRecetteLabel.Text = $"{totalRecette:N2} MAD";
        
        // Parse TPE and Especes values
        decimal.TryParse(TPEEntry.Text, out decimal tpe);
        decimal.TryParse(EspecesEntry.Text, out decimal especes);
        
        TotalTPELabel.Text = $"{tpe:N2} MAD";
        TotalEspecesLabel.Text = $"{especes:N2} MAD";
        
        // Calculate and display ecart
        decimal totalPaiements = tpe + especes;
        decimal ecart = totalRecette - totalPaiements;
        EcartLabel.Text = $"{ecart:N2} MAD";
        
        // Color-code the ecart
        if (Math.Abs(ecart) < 0.01m)
        {
            EcartBorder.BackgroundColor = Color.FromArgb("#E8F5E9"); // Green - balanced
            EcartLabel.TextColor = Color.FromArgb("#2E7D32");
        }
        else if (ecart > 0)
        {
            EcartBorder.BackgroundColor = Color.FromArgb("#FFEBEE"); // Red - missing money
            EcartLabel.TextColor = Color.FromArgb("#C62828");
        }
        else
        {
            EcartBorder.BackgroundColor = Color.FromArgb("#FFF3E0"); // Orange - excess payment
            EcartLabel.TextColor = Color.FromArgb("#E65100");
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;
        
        // Parse TPE and Especes
        decimal.TryParse(TPEEntry.Text, out decimal tpe);
        decimal.TryParse(EspecesEntry.Text, out decimal especes);

        // Create the result object containing periode + all details
        var result = new PeriodeWithDetailsDto
        {
            Periode = new PeriodeDto
            {
                PeriodeID = _existingPeriode?.PeriodeID ?? 0,
                DateDebut = DateTime.SpecifyKind(
                    DateDebutPicker.Date.Add(TimeDebutPicker.Time), 
                    DateTimeKind.Utc),
                DateFin = DateTime.SpecifyKind(
                    DateFinPicker.Date.Add(TimeFinPicker.Time), 
                    DateTimeKind.Utc),
                TPE = tpe,
                Especes = especes
            },
            Details = []
        };

        // Set employee
        if (EmployePicker.SelectedItem is EmployeDto employe)
        {
            result.Periode.EmployeID = employe.ID;
            result.Periode.EmployeNom = employe.Nom;
        }

        // Add all pump readings as details
        foreach (var reading in _viewModel.PompeReadings)
        {
            // Only include pumps with actual sales (quantity > 0)
            if (reading.QuantiteVendue > 0)
            {
                decimal.TryParse(reading.CompteurElecFin, out decimal elecFin);
                decimal.TryParse(reading.CompteurMecaFin, out decimal mecaFin);

                result.Details.Add(new PeriodeDetailsDto
                {
                    PompeID = reading.PompeID,
                    PompeNumero = reading.PompeNumero,
                    ReservoirID = reading.ReservoirID,
                    ReservoirNumero = reading.ReservoirNumero,
                    ProduitID = reading.ProduitID,
                    ProduitNom = reading.ProduitNom,
                    PrixCarburant = reading.PrixCarburant,
                    CompteurElectroniqueDebut = reading.CompteurElecDebut,
                    CompteurElectroniqueFinal = elecFin,
                    CompteurMecaniqueDebut = reading.CompteurMecaDebut,
                    CompteurMecaniqueFinal = mecaFin,
                    QuantiteElectronique = elecFin - reading.CompteurElecDebut,
                    QuantiteMecanique = mecaFin - reading.CompteurMecaDebut,
                    QuantiteVendue = reading.QuantiteVendue,
                    PrixTotal = reading.PrixTotal
                });
            }
        }

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        var debut = DateDebutPicker.Date.Add(TimeDebutPicker.Time);
        var fin = DateFinPicker.Date.Add(TimeFinPicker.Time);

        if (fin <= debut)
        {
            ShowError("La date de fin doit être après la date de début");
            return false;
        }
        
        // Validate TPE value
        if (!string.IsNullOrWhiteSpace(TPEEntry.Text))
        {
            if (!decimal.TryParse(TPEEntry.Text, out decimal tpe) || tpe < 0)
            {
                ShowError("Montant TPE invalide");
                return false;
            }
        }
        
        // Validate Especes value
        if (!string.IsNullOrWhiteSpace(EspecesEntry.Text))
        {
            if (!decimal.TryParse(EspecesEntry.Text, out decimal especes) || especes < 0)
            {
                ShowError("Montant Espèces invalide");
                return false;
            }
        }

        // Validate all readings
        foreach (var reading in _viewModel.PompeReadings)
        {
            if (!string.IsNullOrWhiteSpace(reading.CompteurElecFin))
            {
                if (!decimal.TryParse(reading.CompteurElecFin, out decimal elecFin))
                {
                    ShowError($"Compteur invalide pour pompe {reading.PompeNumero}");
                    return false;
                }

                if (elecFin < reading.CompteurElecDebut)
                {
                    ShowError($"Compteur final < début pour pompe {reading.PompeNumero}");
                    return false;
                }
            }
        }

        // Check if at least one pump has a sale
        if (!_viewModel.PompeReadings.Any(r => r.QuantiteVendue > 0))
        {
            ShowError("Aucune vente enregistrée");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Simultaneously write what's on the CompteurElecFin to CompteurMecaFin      
        if (sender is PompeReadingModel reading && e.PropertyName == nameof(PompeReadingModel.CompteurElecFin))
        {
            reading.CompteurMecaFin = reading.CompteurElecFin;
        }
    }
}