using BombaProMax.Models;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodeCreatePopup : Popup
{
    private readonly PeriodeDto? _existingPeriode;
    private readonly bool _isEditMode;
    private List<EmployeDto> _employes = [];
    private List<PompeDto> _pompes = [];
    private List<ReservoirDto> _reservoirs = [];
    private List<ProduitDto> _produits = [];

    public ObservableCollection<PompeReadingModel> PompeReadings { get; } = [];

    public PeriodeCreatePopup(PeriodeDto? existingPeriode = null)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        BindingContext = this;

        _existingPeriode = existingPeriode;
        _isEditMode = existingPeriode != null;

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
            using var httpClient = new HttpClient();

            // Load all data in parallel
            var employesTask = httpClient.GetStringAsync("https://localhost:7100/api/Employes");
            var pompesTask = httpClient.GetStringAsync("https://localhost:7100/api/Pompes");
            var reservoirsTask = httpClient.GetStringAsync("https://localhost:7100/api/Reservoirs");
            var produitsTask = httpClient.GetStringAsync("https://localhost:7100/api/Produits");

            await Task.WhenAll(employesTask, pompesTask, reservoirsTask, produitsTask);

            _employes = JsonConvert.DeserializeObject<List<EmployeDto>>(await employesTask) ?? [];
            _pompes = JsonConvert.DeserializeObject<List<PompeDto>>(await pompesTask) ?? [];
            _reservoirs = JsonConvert.DeserializeObject<List<ReservoirDto>>(await reservoirsTask) ?? [];
            _produits = JsonConvert.DeserializeObject<List<ProduitDto>>(await produitsTask) ?? [];

            // Set employee picker
            EmployePicker.ItemsSource = _employes;
            if (_isEditMode && _existingPeriode?.EmployeID != null)
            {
                var employe = _employes.FirstOrDefault(e => e.ID == _existingPeriode.EmployeID);
                if (employe != null)
                    EmployePicker.SelectedItem = employe;
            }

            // Build pump readings list
            BuildPompeReadingsList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            ShowError("Erreur de chargement des données");
        }
    }

    private void BuildPompeReadingsList()
    {
        PompeReadings.Clear();

        foreach (var pompe in _pompes.Where(p => p.Statut?.ToLower() == "actif" || p.Statut?.ToLower() == "active"))
        {
            // Find the reservoir for this pump
            var reservoir = _reservoirs.FirstOrDefault(r => r.ID == pompe.ReservoirAssocieID);
            
            // Find the product for this reservoir
            ProduitDto? produit = null;
            decimal prix = 0;
            if (reservoir?.ProduitID != null)
            {
                produit = _produits.FirstOrDefault(p => p.ID == reservoir.ProduitID);
                // Get price from product (use PrixTTC as selling price)
                prix = produit?.PrixTTC ?? 0;
            }

            var reading = new PompeReadingModel
            {
                PompeID = pompe.ID,
                PompeNumero = pompe.Numero,
                ReservoirID = pompe.ReservoirAssocieID,
                ReservoirNumero = reservoir?.Numero,
                ProduitID = produit?.ID,
                ProduitNom = produit?.Description ?? reservoir?.ProduitNom ?? "N/A",
                PrixCarburant = prix,
                CompteurElecDebut = pompe.CompteurElectroniqueActuel ?? 0,
                CompteurMecaDebut = pompe.CompteurMecaniqueActuel ?? 0,
                // Pre-fill final with same as start (user will update)
                CompteurElecFin = (pompe.CompteurElectroniqueActuel ?? 0).ToString("F2"),
                CompteurMecaFin = (pompe.CompteurMecaniqueActuel ?? 0).ToString("F2")
            };

            // Subscribe to property changes for summary updates
            reading.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PompeReadingModel.QuantiteVendue))
                {
                    UpdateSummary();
                }
            };

            PompeReadings.Add(reading);
        }

        UpdateSummary();
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

        foreach (var reading in PompeReadings)
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
        foreach (var reading in PompeReadings)
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
        foreach (var reading in PompeReadings)
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
        if (!PompeReadings.Any(r => r.QuantiteVendue > 0))
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