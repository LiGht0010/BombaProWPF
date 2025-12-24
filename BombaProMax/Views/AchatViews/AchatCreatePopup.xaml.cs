using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.AchatViews;

public partial class AchatCreatePopup : Popup
{
    private readonly AchatService _achatService;
    private readonly FournisseurService _fournisseurService;
    private readonly ProduitService _produitService;
    private readonly CamionService _camionService;
    private readonly ChauffeurService _chauffeurService;

    private List<FournisseurDto> _fournisseurs = [];
    private List<ProduitDto> _produits = [];
    private List<ChauffeurDto> _chauffeurs = [];
    private List<CamionDto> _camions = [];

    public AchatCreatePopup(
        AchatService achatService,
        FournisseurService fournisseurService,
        ProduitService produitService,
        CamionService camionService,
        ChauffeurService chauffeurService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _achatService = achatService;
        _fournisseurService = fournisseurService;
        _produitService = produitService;
        _camionService = camionService;
        _chauffeurService = chauffeurService;

        // Set default date to today
        AchatDatePicker.Date = DateTime.Now.Date;

        // Load data and generate numero
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await GenerateAchatNumberAsync();
        await LoadPickerDataAsync();
    }

    private async Task GenerateAchatNumberAsync()
    {
        try
        {
            var numero = await _achatService.GenerateNextNumeroAsync();
            NumeroEntry.Text = numero;
        }
        catch
        {
            // Fallback to simple generation
            var today = DateTime.Now;
            var random = new Random().Next(100, 999);
            NumeroEntry.Text = $"ACH-{today:yyyyMMdd}-{random}";
        }
    }

    private async Task LoadPickerDataAsync()
    {
        try
        {
            // Load Fournisseurs
            _fournisseurs = await _fournisseurService.GetActiveFournisseursAsync();
            FournisseurPicker.ItemsSource = _fournisseurs;

            // Load Produits - filter out products without description
            var allProduits = await _produitService.GetAllProduitsAsync();
            _produits = allProduits
                .Where(p => !string.IsNullOrWhiteSpace(p.Description) || !string.IsNullOrWhiteSpace(p.NumeroProduit))
                .ToList();
            ProduitPicker.ItemsSource = _produits;

            // Load Chauffeurs and set initial source
            _chauffeurs = await _chauffeurService.GetAllChauffeursAsync();
            ChauffeurPicker.ItemsSource = _chauffeurs;

            // Load Camions
            var allCamions = await _camionService.GetAllCamionsAsync();
            _camions = allCamions
                .Where(p => !string.IsNullOrWhiteSpace(p.Matricule) || !string.IsNullOrWhiteSpace(p.Marque))
                .ToList();
            CamionPicker.ItemsSource = _camions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading picker data: {ex.Message}");
        }
    }

    private void OnFournisseurChanged(object? sender, EventArgs e)
    {
        // Filter chauffeurs by selected fournisseur
        if (FournisseurPicker.SelectedItem is FournisseurDto selectedFournisseur)
        {
            var filteredChauffeurs = _chauffeurs
                .Where(c => c.FournisseurID == selectedFournisseur.ID || c.FournisseurID == null)
                .ToList();
            ChauffeurPicker.ItemsSource = filteredChauffeurs;

            // Also filter camions by fournisseur if applicable
            var filteredCamions = _camions
                .Where(c => c.FournisseurID == selectedFournisseur.ID || c.FournisseurID == null)
                .ToList();
            CamionPicker.ItemsSource = filteredCamions;
        }
    }

    private void OnProduitChanged(object? sender, EventArgs e)
    {
        if (ProduitPicker.SelectedItem is ProduitDto selectedProduit)
        {
            // Auto-fill the price field with the product's PrixAchat
            if (selectedProduit.PrixAchat.HasValue)
            {
                PrixUnitaireEntry.Text = selectedProduit.PrixAchat.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                PrixUnitaireEntry.Text = string.Empty;
            }
            CalculateTotalCost();
        }
    }

    private void OnQuantityOrPriceChanged(object? sender, TextChangedEventArgs e)
    {
        CalculateTotalCost();
    }

    private void CalculateTotalCost()
    {
        try
        {
            var quantityText = QuantiteEntry.Text?.Replace(",", ".");
            var priceText = PrixUnitaireEntry.Text?.Replace(",", ".");

            if (decimal.TryParse(quantityText, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var quantity) &&
                decimal.TryParse(priceText, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out var price))
            {
                var total = quantity * price;
                CoutTotalLabel.Text = $"{total:N2} DH";
            }
            else
            {
                CoutTotalLabel.Text = "0.00 DH";
            }
        }
        catch
        {
            CoutTotalLabel.Text = "0.00 DH";
        }
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        try
        {
            ErrorLabel.IsVisible = false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(NumeroEntry.Text))
            {
                ShowError("Le numéro d'achat est requis");
                return;
            }

            if (FournisseurPicker.SelectedItem is not FournisseurDto selectedFournisseur)
            {
                ShowError("Veuillez sélectionner un fournisseur");
                return;
            }

            if (ProduitPicker.SelectedItem is not ProduitDto selectedProduit)
            {
                ShowError("Veuillez sélectionner un produit");
                return;
            }

            var quantityText = QuantiteEntry.Text?.Replace(",", ".");
            if (!int.TryParse(quantityText, out var quantity) || quantity <= 0)
            {
                ShowError("Veuillez entrer une quantité valide");
                return;
            }

            var priceText = PrixUnitaireEntry.Text?.Replace(",", ".");
            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var prixUnitaire) || prixUnitaire <= 0)
            {
                ShowError("Veuillez entrer un prix unitaire valide");
                return;
            }

            // Check if numero already exists
            var numeroExists = await _achatService.AchatNumberExistsAsync(NumeroEntry.Text.Trim());
            if (numeroExists)
            {
                ShowError("Un achat avec ce numéro existe déjà");
                return;
            }

            // Calculate total cost
            var coutTotal = quantity * prixUnitaire;

            // Create new Achat DTO
            var newAchat = new AchatDto
            {
                Numero = NumeroEntry.Text.Trim(),
                Date = DateOnly.FromDateTime(AchatDatePicker.Date),
                FournisseurID = selectedFournisseur.ID,
                FournisseurNom = selectedFournisseur.Nom,
                ProduitID = selectedProduit.ID,
                ProduitNom = selectedProduit.Description ?? selectedProduit.NumeroProduit,
                Quantite = quantity,
                PrixAchatUnitaire = prixUnitaire,
                Cout = coutTotal,
                LivraisonDefectueuse = LivraisonDefectueuseSwitch.IsToggled
            };

            // Set optional fields
            if (ChauffeurPicker.SelectedItem is ChauffeurDto selectedChauffeur)
            {
                newAchat.ChauffeurID = selectedChauffeur.ID;
                newAchat.ChauffeurNom = selectedChauffeur.Nom;
            }

            if (CamionPicker.SelectedItem is CamionDto selectedCamion)
            {
                newAchat.CamionID = selectedCamion.ID;
                newAchat.CamionImmatriculation = selectedCamion.Matricule;
            }

            // Save to database via service
            var createdAchat = await _achatService.CreateAsync(newAchat);

            if (createdAchat != null)
            {
                await CloseAsync(createdAchat);
            }
            else
            {
                ShowError("Erreur lors de la création de l'achat");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }
}