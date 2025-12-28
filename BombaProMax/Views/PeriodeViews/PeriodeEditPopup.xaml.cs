using BombaProMax.Models;
using BombaProMax.ViewModels;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodeEditPopup : Popup
{
    private readonly PeriodeDto _periode;
    private readonly List<PeriodeDetailsDto> _existingDetails;
    private readonly PeriodeViewModel _viewModel;
    private bool _isLoading = true;

    public PeriodeEditPopup(PeriodeDto periode, List<PeriodeDetailsDto> existingDetails, PeriodeViewModel viewModel)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _periode = periode ?? throw new ArgumentNullException(nameof(periode));
        _existingDetails = existingDetails ?? [];
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        
        // Clear stale data from shared ViewModel immediately
        _viewModel.PompeReadings.Clear();
        _viewModel.ClearCreditTransactions();
        
        BindingContext = _viewModel;

        // Set date/time values from existing periode
        DateDebutPicker.Date = _periode.DateDebut.Date;
        TimeDebutPicker.Time = _periode.DateDebut.TimeOfDay;
        DateFinPicker.Date = _periode.DateFin.Date;
        TimeFinPicker.Time = _periode.DateFin.TimeOfDay;
        
        // Set payment values
        TPEEntry.Text = _periode.TPE.ToString("F2");
        EspecesEntry.Text = _periode.Especes.ToString("F2");

        // Disable save button until data is loaded
        SaveButton.IsEnabled = false;
        SaveButton.Text = "? Chargement...";

        System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Opening edit for Periode {_periode.PeriodeID} with {_existingDetails.Count} existing details");

        // Load all data
        _ = LoadAllDataAsync();
    }

    private async Task LoadAllDataAsync()
    {
        try
        {
            _isLoading = true;
            
            System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Loading data for Periode {_periode.PeriodeID}");

            // Load reference data through ViewModel
            await _viewModel.LoadReferenceDataAsync();

            // Set employee picker
            EmployePicker.ItemsSource = _viewModel.Employes.ToList();
            if (_periode.EmployeID.HasValue)
            {
                var employe = _viewModel.Employes.FirstOrDefault(e => e.ID == _periode.EmployeID.Value);
                if (employe != null)
                    EmployePicker.SelectedItem = employe;
            }

            // Build pump readings list with existing values
            _viewModel.BuildPompeReadingsForEdit(_existingDetails);
            
            System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Built {_viewModel.PompeReadings.Count} pump readings from {_existingDetails.Count} existing details");
            
            // Bind collection view AFTER data is loaded
            PompesCollectionView.ItemsSource = _viewModel.PompeReadings;

            // Load credit transactions: already linked + unassigned within date range
            await _viewModel.LoadCreditTransactionsForEditAsync(
                _periode.PeriodeID, 
                _periode.DateDebut, 
                _periode.DateFin);
            
            System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Loaded {_viewModel.PeriodeCreditTransactions.Count} credit transactions");
            
            // Bind credit transactions collection AFTER data is loaded
            CreditTransactionsCollectionView.ItemsSource = _viewModel.PeriodeCreditTransactions;

            UpdateCreditSummary();
            UpdateSummary();
            
            // Enable save button now that data is loaded
            _isLoading = false;
            SaveButton.IsEnabled = true;
            SaveButton.Text = "? Enregistrer les Modifications";
            
            System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Data loading complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Error loading data: {ex.Message}");
            _isLoading = false;
            SaveButton.IsEnabled = true;
            SaveButton.Text = "? Enregistrer les Modifications";
            ShowError("Erreur de chargement des données");
        }
    }

    private void OnMeterValueChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isLoading)
            UpdateSummary();
    }
    
    private void OnPaymentValueChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isLoading)
            UpdateSummary();
    }

    private void OnPropChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoading) return;
        
        // Simultaneously write what's on the CompteurElecFin to CompteurMecaFin      
        if (sender is Entry entry && entry.BindingContext is PompeReadingModel reading 
            && e.PropertyName == nameof(Entry.Text))
        {
            // Only sync if this is the Elec Fin entry (check by background color)
            if (entry.BackgroundColor == Color.FromArgb("#FFF3E0"))
            {
                reading.CompteurMecaFin = reading.CompteurElecFin;
            }
        }
    }

    private void OnCreditTransactionTapped(object? sender, TappedEventArgs e)
    {
        if (_isLoading) return;
        
        if (e.Parameter is CreditTransactionDto ct)
        {
            _viewModel.ToggleCreditTransactionSelection(ct);
            UpdateCreditSummary();
        }
    }

    private void OnCreditCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!_isLoading)
            UpdateCreditSummary();
    }

    private void OnSelectAllCreditsClicked(object? sender, EventArgs e)
    {
        if (_isLoading) return;
        
        _viewModel.SelectAllCreditTransactions();
        UpdateCreditSummary();
    }

    private void OnDeselectAllCreditsClicked(object? sender, EventArgs e)
    {
        if (_isLoading) return;
        
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
        // Prevent saving while still loading
        if (_isLoading)
        {
            ShowError("Veuillez attendre le chargement des données");
            return;
        }
        
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
                PeriodeID = _periode.PeriodeID,
                DateDebut = DateTime.SpecifyKind(
                    DateDebutPicker.Date.Add(TimeDebutPicker.Time), 
                    DateTimeKind.Utc),
                DateFin = DateTime.SpecifyKind(
                    DateFinPicker.Date.Add(TimeFinPicker.Time), 
                    DateTimeKind.Utc),
                TPE = tpe,
                Especes = especes,
                // Preserve audit fields
                AjoutePar = _periode.AjoutePar,
                DateCreation = _periode.DateCreation
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
            decimal.TryParse(reading.CompteurElecFin, out decimal elecFin);
            decimal.TryParse(reading.CompteurMecaFin, out decimal mecaFin);
            
            // Calculate quantity
            var quantiteVendue = elecFin - reading.CompteurElecDebut;
            
            // Check if this pump had an existing detail
            var existingDetail = _existingDetails.FirstOrDefault(d => d.PompeID == reading.PompeID);
            
            // Include pump if: has sales (quantity > 0) OR had an existing detail (preserve data)
            if (quantiteVendue > 0 || existingDetail != null)
            {
                result.Details.Add(new PeriodeDetailsDto
                {
                    PeriodeDetailID = existingDetail?.PeriodeDetailID ?? 0,
                    PeriodeID = _periode.PeriodeID,
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
                    QuantiteVendue = Math.Max(0, quantiteVendue),
                    PrixTotal = Math.Max(0, quantiteVendue) * reading.PrixCarburant
                });
            }
        }

        System.Diagnostics.Debug.WriteLine($"[PeriodeEditPopup] Saving periode {_periode.PeriodeID} with {result.Details.Count} details (from {_existingDetails.Count} existing) and {result.CreditTransactionIds.Count} CTs");
        
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

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}