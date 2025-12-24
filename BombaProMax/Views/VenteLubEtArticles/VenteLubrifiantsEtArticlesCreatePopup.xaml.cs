using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteLubEtArticles;

public partial class VenteLubrifiantsEtArticlesCreatePopup : Popup
{
    private readonly VenteLubrifiantsEtArticlesDto? _existingVente;
    private readonly bool _isEditMode;
    private readonly ProduitService _produitService;
    private readonly ClientService _clientService;
    private readonly EmployeService _employeService;
    private readonly MoyensPaiementService _moyensPaiementService;
    
    private List<ProduitDto> _produits = [];
    private List<ClientDto> _clients = [];
    private List<EmployeDto> _employes = [];
    private List<MoyensPaiementDto> _moyensPaiement = [];
    private ProduitDto? _selectedProduit;

    private const decimal TVA_RATE = 0.2M;

    public VenteLubrifiantsEtArticlesCreatePopup(VenteLubrifiantsEtArticlesDto? existingVente = null)
    {
        InitializeComponent();

        _existingVente = existingVente;
        _isEditMode = existingVente != null;
        
        // Initialize services
        _produitService = new ProduitService();
        _clientService = new ClientService();
        _employeService = new EmployeService();
        _moyensPaiementService = new MoyensPaiementService();

        // Set default date/time
        var now = DateTime.Now;
        DateVentePicker.Date = now.Date;
        TimeVentePicker.Time = new TimeSpan(now.Hour, now.Minute, 0);

        if (_isEditMode && existingVente != null)
        {
            HeaderLabel.Text = "??? Modifier la Vente";
            SaveButton.Text = "? Mettre à jour";
            PopulateForEdit(existingVente);
        }

        // Load all reference data
        _ = LoadAllDataAsync();
    }

    private void PopulateForEdit(VenteLubrifiantsEtArticlesDto vente)
    {
        DateVentePicker.Date = vente.DateVente.Date;
        TimeVentePicker.Time = vente.DateVente.TimeOfDay;
        NumeroVenteEntry.Text = vente.NumeroVente;
        QuantiteEntry.Text = vente.QuantiteVendue.ToString();
        PrixUnitaireEntry.Text = vente.PrixUnitaireTTC.ToString("F2");
        NotesEditor.Text = vente.Notes;
    }

    private async Task LoadAllDataAsync()
    {
        try
        {
            // Load all data in parallel using existing services
            var produitsTask = _produitService.GetAllProduitsAsync();
            var clientsTask = _clientService.GetAllClientsAsync();
            var employesTask = _employeService.GetAllEmployesAsync();
            var moyensPaiementTask = _moyensPaiementService.GetAllAsync();

            await Task.WhenAll(produitsTask, clientsTask, employesTask, moyensPaiementTask);

            var allProduits = await produitsTask;
            _clients = await clientsTask;
            _employes = await employesTask;
            _moyensPaiement = await moyensPaiementTask;

            // Filter products to exclude fuel (carburant)
            _produits = allProduits.Where(p =>
                p.CategorieNom != null &&
                !p.CategorieNom.Equals("Carburant", StringComparison.OrdinalIgnoreCase) &&
                !p.CategorieNom.Equals("Fuel", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.CategorieNom)
                .ThenBy(p => p.Description)
                .ToList();

            // Populate pickers
            ProduitPicker.ItemsSource = _produits;
            ClientPicker.ItemsSource = _clients;
            EmployePicker.ItemsSource = _employes;
            MoyenPaiementPicker.ItemsSource = _moyensPaiement;

            // If editing, set selected items
            if (_isEditMode && _existingVente != null)
            {
                var produit = _produits.FirstOrDefault(p => p.ID == _existingVente.ProduitID);
                if (produit != null)
                {
                    ProduitPicker.SelectedItem = produit;
                    _selectedProduit = produit;
                    UpdateProductInfo();
                }

                if (_existingVente.ClientID.HasValue)
                {
                    var client = _clients.FirstOrDefault(c => c.ID == _existingVente.ClientID);
                    if (client != null)
                        ClientPicker.SelectedItem = client;
                }

                if (_existingVente.EmployeID.HasValue)
                {
                    var employe = _employes.FirstOrDefault(e => e.ID == _existingVente.EmployeID);
                    if (employe != null)
                        EmployePicker.SelectedItem = employe;
                }

                if (_existingVente.MoyenPaiementID.HasValue)
                {
                    var moyen = _moyensPaiement.FirstOrDefault(m => m.ID == _existingVente.MoyenPaiementID);
                    if (moyen != null)
                        MoyenPaiementPicker.SelectedItem = moyen;
                }

                UpdateTotals();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            ShowError("Erreur de chargement des données");
        }
    }

    private void OnProduitSelected(object? sender, EventArgs e)
    {
        if (ProduitPicker.SelectedItem is ProduitDto produit)
        {
            _selectedProduit = produit;
            UpdateProductInfo();

            // Auto-fill price from product
            if (produit.PrixTTC.HasValue)
            {
                PrixUnitaireEntry.Text = produit.PrixTTC.Value.ToString("F2");
            }

            UpdateTotals();
        }
    }

    private void UpdateProductInfo()
    {
        if (_selectedProduit != null)
        {
            ProductInfoCard.IsVisible = true;
            PrixTTCLabel.Text = (_selectedProduit.PrixTTC ?? 0).ToString("F2");
            StockLabel.Text = (_selectedProduit.Stock ?? 0).ToString();
            CategorieLabel.Text = _selectedProduit.CategorieNom ?? "-";
            StockMinLabel.Text = (_selectedProduit.StockMinimum ?? 0).ToString();

            // Highlight low stock
            if (_selectedProduit.Stock.HasValue && _selectedProduit.StockMinimum.HasValue &&
                _selectedProduit.Stock.Value <= _selectedProduit.StockMinimum.Value)
            {
                StockLabel.TextColor = Color.FromArgb("#C62828");
            }
            else
            {
                StockLabel.TextColor = Color.FromArgb("#7B1FA2");
            }
        }
        else
        {
            ProductInfoCard.IsVisible = false;
        }
    }

    private void OnQuantiteChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateTotals();
    }

    private void OnPrixChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        decimal quantite = 0;
        decimal prixUnitaireTTC = 0;

        int.TryParse(QuantiteEntry.Text, out int qte);
        quantite = qte;

        decimal.TryParse(PrixUnitaireEntry.Text, out prixUnitaireTTC);

        var totalTTC = quantite * prixUnitaireTTC;
        var totalHT = totalTTC / (1 + TVA_RATE);
        var tva = totalTTC - totalHT;

        TotalHTLabel.Text = $"{totalHT:N2} MAD";
        TVALabel.Text = $"{tva:N2} MAD";
        TotalTTCLabel.Text = $"{totalTTC:N2} MAD";
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        var produit = ProduitPicker.SelectedItem as ProduitDto;
        var client = ClientPicker.SelectedItem as ClientDto;
        var employe = EmployePicker.SelectedItem as EmployeDto;
        var moyenPaiement = MoyenPaiementPicker.SelectedItem as MoyensPaiementDto;

        int.TryParse(QuantiteEntry.Text, out int quantite);
        decimal.TryParse(PrixUnitaireEntry.Text, out decimal prixUnitaireTTC);

        var result = new VenteLubrifiantsEtArticlesDto
        {
            ID = _existingVente?.ID ?? 0,
            NumeroVente = _existingVente?.NumeroVente, // Will be auto-generated for new sales
            DateVente = DateTime.SpecifyKind(
                DateVentePicker.Date.Add(TimeVentePicker.Time),
                DateTimeKind.Utc),
            ProduitID = produit!.ID,
            ProduitNom = produit.Description,
            QuantiteVendue = quantite,
            PrixUnitaireTTC = prixUnitaireTTC,
            ClientID = client?.ID,
            ClientNom = client?.Nom,
            EmployeID = employe?.ID,
            EmployeNom = employe?.Nom,
            MoyenPaiementID = moyenPaiement?.ID,
            MoyenPaiementNom = moyenPaiement?.Nom,
            Notes = NotesEditor.Text,
            Statut = "Confirmée",
            CategorieNom = produit.CategorieNom ?? ""
        };

        // Calculate computed properties
        result.MontantTotalTTC = result.QuantiteVendue * result.PrixUnitaireTTC;
        result.PrixUnitaireHT = result.PrixUnitaireTTC / (1 + TVA_RATE);
        result.MontantTotalHT = result.PrixUnitaireHT * result.QuantiteVendue;
        result.MontantTVA = result.MontantTotalTTC - result.MontantTotalHT;

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;
        ErrorBorder.IsVisible = false;

        // Validate product selection
        if (ProduitPicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un produit");
            return false;
        }

        // Validate quantity
        if (!int.TryParse(QuantiteEntry.Text, out int quantite) || quantite <= 0)
        {
            ShowError("Quantité invalide (doit être > 0)");
            return false;
        }

        // Check stock availability
        if (_selectedProduit?.Stock.HasValue == true)
        {
            var availableStock = _selectedProduit.Stock.Value;
            
            // If editing, add back the original quantity
            if (_isEditMode && _existingVente != null && _existingVente.ProduitID == _selectedProduit.ID)
            {
                availableStock += _existingVente.QuantiteVendue;
            }

            if (quantite > availableStock)
            {
                ShowError($"Stock insuffisant (disponible: {availableStock})");
                return false;
            }
        }

        // Validate price
        if (!decimal.TryParse(PrixUnitaireEntry.Text, out decimal prix) || prix <= 0)
        {
            ShowError("Prix unitaire invalide (doit être > 0)");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
        ErrorBorder.IsVisible = true;
    }
}