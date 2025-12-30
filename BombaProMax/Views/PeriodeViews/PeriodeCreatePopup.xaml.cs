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
        var today = DateTime.Today;
        DateDebutPicker.Date = today.AddDays(-1); // Yesterday
        TimeDebutPicker.Time = new TimeSpan(10, 0, 0); // 10:00 AM
        DateFinPicker.Date = today; // Today
        TimeFinPicker.Time = new TimeSpan(10, 0, 0); // 10:00 AM
        
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

            // Load credit transactions
            if (_isEditMode && _existingPeriode != null)
            {
                // Edit mode: load CTs linked to this periode
                await _viewModel.LoadCreditTransactionsByPeriodeAsync(_existingPeriode.PeriodeID);
            }
            else
            {
                // Create mode: load CTs by date range
                await LoadCreditTransactionsForDateRange();
            }

            // Bind credit transactions collection
            CreditTransactionsCollectionView.ItemsSource = _viewModel.PeriodeCreditTransactions;

            UpdateSummary();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            ShowError("Erreur de chargement des données");
        }
    }

    private async Task LoadCreditTransactionsForDateRange()
    {
        var start = DateDebutPicker.Date.Add(TimeDebutPicker.Time);
        var end = DateFinPicker.Date.Add(TimeFinPicker.Time);
        
        await _viewModel.LoadCreditTransactionsByDateRangeAsync(start, end);
        UpdateCreditSummary();
    }

    private void OnDateRangeChanged(object? sender, DateChangedEventArgs e)
    {
        // Only reload CTs in create mode when date changes
        if (!_isEditMode)
        {
            _ = LoadCreditTransactionsForDateRange();
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

    private void OnCreditTransactionTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is CreditTransactionDto ct)
        {
            _viewModel.ToggleCreditTransactionSelection(ct);
            UpdateCreditSummary();
        }
    }

    private void OnCreditCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        UpdateCreditSummary();
    }

    private void OnSelectAllCreditsClicked(object? sender, EventArgs e)
    {
        _viewModel.SelectAllCreditTransactions();
        UpdateCreditSummary();
    }

    private void OnDeselectAllCreditsClicked(object? sender, EventArgs e)
    {
        _viewModel.DeselectAllCreditTransactions();
        UpdateCreditSummary();
    }

    private void UpdateCreditSummary()
    {
        var totalCredite = _viewModel.TotalCredite;
        var selectedCount = _viewModel.SelectedCreditTransactionsCount;
        
        TotalCrediteLabel.Text = $"{totalCredite:N2} MAD";
        TotalCrediteSummaryLabel.Text = $"{totalCredite:N2}";
        CreditCountLabel.Text = $"{selectedCount} sélectionné(s)";
        
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

        TotalRecetteLabel.Text = $"{totalRecette:N2}";
        
        // Parse TPE and Especes values
        decimal.TryParse(TPEEntry.Text, out decimal tpe);
        decimal.TryParse(EspecesEntry.Text, out decimal especes);
        
        TotalTPELabel.Text = $"{tpe:N2}";
        TotalEspecesLabel.Text = $"{especes:N2}";
        
        // Get total credited from selected transactions
        decimal totalCredite = _viewModel.TotalCredite;
        TotalCrediteSummaryLabel.Text = $"{totalCredite:N2}";
        
        // Calculate Espèces Attendues = Recette - TPE - Crédité
        decimal especesAttendues = totalRecette - tpe - totalCredite;
        EspecesAttenduesLabel.Text = $"{especesAttendues:N2}";
        
        // Calculate Manque = Espèces Attendues - Espèces Déclarées
        decimal manque = especesAttendues - especes;
        ManqueLabel.Text = $"{manque:N2}";
        
        // Color-code the manque
        if (Math.Abs(manque) < 0.01m)
        {
            ManqueBorder.BackgroundColor = Color.FromArgb("#E8F5E9"); // Green - balanced
            ManqueLabel.TextColor = Color.FromArgb("#2E7D32");
        }
        else if (manque > 0)
        {
            ManqueBorder.BackgroundColor = Color.FromArgb("#FFEBEE"); // Red - missing money
            ManqueLabel.TextColor = Color.FromArgb("#C62828");
        }
        else
        {
            ManqueBorder.BackgroundColor = Color.FromArgb("#E3F2FD"); // Blue - excess payment
            ManqueLabel.TextColor = Color.FromArgb("#1565C0");
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

        // Create the result object containing periode + all details + credit transaction IDs
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
            Details = [],
            CreditTransactionIds = _viewModel.GetSelectedCreditTransactionIds()
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