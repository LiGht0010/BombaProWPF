using BombaProMax.Models;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodeDetailsPopup : Popup
{
    private readonly int _periodeId;
    private readonly PeriodeDetailsDto? _existingDetail;
    private readonly bool _isEditMode;

    private List<PompeDto> _pompes = [];
    private List<ProduitDto> _produits = [];

    public PeriodeDetailsPopup(int periodeId, PeriodeDetailsDto? existingDetail = null)
    {
        InitializeComponent();

        _periodeId = periodeId;
        _existingDetail = existingDetail;
        _isEditMode = existingDetail != null;

        if (_isEditMode)
        {
            HeaderLabel.Text = "?? Modifier Relevé";
            SaveButton.Text = "? Mettre à jour";
        }

        // Load data
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(LoadPompesAsync(), LoadProduitsAsync());

        if (_isEditMode && _existingDetail != null)
        {
            PopulateForEdit(_existingDetail);
        }
    }

    private async Task LoadPompesAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7100/api/Pompes");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _pompes = JsonConvert.DeserializeObject<List<PompeDto>>(json) ?? [];
                PompePicker.ItemsSource = _pompes;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading pompes: {ex.Message}");
        }
    }

    private async Task LoadProduitsAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7100/api/Produits");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _produits = JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? [];
                ProduitPicker.ItemsSource = _produits;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading produits: {ex.Message}");
        }
    }

    private void PopulateForEdit(PeriodeDetailsDto detail)
    {
        // Set pompe
        var pompe = _pompes.FirstOrDefault(p => p.ID == detail.PompeID);
        if (pompe != null)
        {
            PompePicker.SelectedItem = pompe;
            ReservoirLabel.Text = pompe.ReservoirNumero ?? "N/A";
        }

        // Set produit
        var produit = _produits.FirstOrDefault(p => p.ID == detail.ProduitID);
        if (produit != null)
        {
            ProduitPicker.SelectedItem = produit;
        }

        // Set values
        PrixCarburantEntry.Text = detail.PrixCarburant.ToString("F2");
        CompteurElecDebutEntry.Text = detail.CompteurElectroniqueDebut.ToString("F2");
        CompteurElecFinalEntry.Text = detail.CompteurElectroniqueFinal.ToString("F2");
        CompteurMecaDebutEntry.Text = detail.CompteurMecaniqueDebut.ToString("F2");
        CompteurMecaFinalEntry.Text = detail.CompteurMecaniqueFinal.ToString("F2");

        UpdateCalculations();
    }

    private void OnPompeSelected(object? sender, EventArgs e)
    {
        if (PompePicker.SelectedItem is PompeDto pompe)
        {
            ReservoirLabel.Text = pompe.ReservoirNumero ?? "N/A";

            // Auto-fill starting counters from pompe's current values
            if (!_isEditMode)
            {
                if (pompe.CompteurElectroniqueActuel.HasValue)
                {
                    CompteurElecDebutEntry.Text = pompe.CompteurElectroniqueActuel.Value.ToString("F2");
                }
                if (pompe.CompteurMecaniqueActuel.HasValue)
                {
                    CompteurMecaDebutEntry.Text = pompe.CompteurMecaniqueActuel.Value.ToString("F2");
                }
            }
        }
        else
        {
            ReservoirLabel.Text = "Sélectionnez une pompe";
        }
    }

    private void OnProduitSelected(object? sender, EventArgs e)
    {
        if (ProduitPicker.SelectedItem is ProduitDto produit)
        {
            // Auto-fill price if product has a default price
            // You might want to add a Prix property to ProduitDto
        }
    }

    private void OnValuesChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateCalculations();
    }

    private void UpdateCalculations()
    {
        // Parse values
        decimal.TryParse(CompteurElecDebutEntry.Text, out decimal elecDebut);
        decimal.TryParse(CompteurElecFinalEntry.Text, out decimal elecFinal);
        decimal.TryParse(CompteurMecaDebutEntry.Text, out decimal mecaDebut);
        decimal.TryParse(CompteurMecaFinalEntry.Text, out decimal mecaFinal);
        decimal.TryParse(PrixCarburantEntry.Text, out decimal prixCarburant);

        // Calculate quantities
        decimal qteElec = elecFinal - elecDebut;
        decimal qteMeca = mecaFinal - mecaDebut;
        decimal difference = Math.Abs(qteElec - qteMeca);

        // Update labels
        QteElecLabel.Text = $"{qteElec:N2} L";
        QteElecLabel.TextColor = qteElec >= 0 ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828");

        QteMecaLabel.Text = $"{qteMeca:N2} L";
        QteMecaLabel.TextColor = qteMeca >= 0 ? Color.FromArgb("#E65100") : Color.FromArgb("#C62828");

        // Difference section
        if (difference > 0.01m)
        {
            DifferenceSection.IsVisible = true;
            DifferenceLabel.Text = $"{difference:N2} L";

            // Color based on percentage
            var maxQte = Math.Max(Math.Abs(qteElec), Math.Abs(qteMeca));
            if (maxQte > 0)
            {
                var percent = (difference / maxQte) * 100;
                DifferenceLabel.TextColor = percent > 5 ? Color.FromArgb("#C62828") : Color.FromArgb("#F57C00");
            }
        }
        else
        {
            DifferenceSection.IsVisible = false;
        }

        // Summary (use electronic as primary)
        decimal qteVendue = qteElec;
        decimal prixTotal = qteVendue * prixCarburant;

        QuantiteVendueLabel.Text = $"{qteVendue:N2} L";
        PrixTotalLabel.Text = $"{prixTotal:N2} MAD";
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        var detail = _existingDetail ?? new PeriodeDetailsDto();

        detail.PeriodeID = _periodeId;

        // Pompe
        if (PompePicker.SelectedItem is PompeDto pompe)
        {
            detail.PompeID = pompe.ID;
            detail.PompeNumero = pompe.Numero;
            detail.ReservoirID = pompe.ReservoirAssocieID;
            detail.ReservoirNumero = pompe.ReservoirNumero;
        }

        // Produit
        if (ProduitPicker.SelectedItem is ProduitDto produit)
        {
            detail.ProduitID = produit.ID;
            detail.ProduitNom = produit.Description;
        }

        // Values
        detail.PrixCarburant = decimal.Parse(PrixCarburantEntry.Text);
        detail.CompteurElectroniqueDebut = decimal.Parse(CompteurElecDebutEntry.Text);
        detail.CompteurElectroniqueFinal = decimal.Parse(CompteurElecFinalEntry.Text);
        detail.CompteurMecaniqueDebut = decimal.Parse(CompteurMecaDebutEntry.Text);
        detail.CompteurMecaniqueFinal = decimal.Parse(CompteurMecaFinalEntry.Text);

        // Calculated fields (also calculated server-side, but set for immediate display)
        detail.QuantiteElectronique = detail.CompteurElectroniqueFinal - detail.CompteurElectroniqueDebut;
        detail.QuantiteMecanique = detail.CompteurMecaniqueFinal - detail.CompteurMecaniqueDebut;
        detail.DifferenceQuantite = Math.Abs(detail.QuantiteElectronique - detail.QuantiteMecanique);
        detail.QuantiteVendue = detail.QuantiteElectronique;
        detail.PrixTotal = detail.QuantiteVendue * detail.PrixCarburant;

        Close(detail);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        if (PompePicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner une pompe");
            return false;
        }

        if (ProduitPicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un produit");
            return false;
        }

        if (!decimal.TryParse(PrixCarburantEntry.Text, out decimal prix) || prix <= 0)
        {
            ShowError("Veuillez entrer un prix valide (> 0)");
            return false;
        }

        if (!decimal.TryParse(CompteurElecDebutEntry.Text, out _))
        {
            ShowError("Compteur électronique début invalide");
            return false;
        }

        if (!decimal.TryParse(CompteurElecFinalEntry.Text, out decimal elecFinal))
        {
            ShowError("Compteur électronique final invalide");
            return false;
        }

        if (!decimal.TryParse(CompteurMecaDebutEntry.Text, out _))
        {
            ShowError("Compteur mécanique début invalide");
            return false;
        }

        if (!decimal.TryParse(CompteurMecaFinalEntry.Text, out _))
        {
            ShowError("Compteur mécanique final invalide");
            return false;
        }

        // Validate that final > debut
        decimal.TryParse(CompteurElecDebutEntry.Text, out decimal elecDebut);
        if (elecFinal < elecDebut)
        {
            ShowError("Le compteur électronique final doit être ? au début");
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