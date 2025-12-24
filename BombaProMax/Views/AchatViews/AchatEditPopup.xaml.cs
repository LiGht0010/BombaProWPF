using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.AchatViews;

public partial class AchatEditPopup : Popup
{
    private readonly AchatService _achatService;
    private readonly FournisseurService _fournisseurService;
    private readonly ProduitService _produitService;
    private readonly CamionService _camionService;
    private readonly ChauffeurService _chauffeurService;

    private readonly AchatDto _originalAchat;

    private List<FournisseurDto> _fournisseurs = [];
    private List<ProduitDto> _produits = [];
    private List<ChauffeurDto> _chauffeurs = [];
    private List<CamionDto> _camions = [];

    public AchatEditPopup(
        AchatService achatService,
        FournisseurService fournisseurService,
        ProduitService produitService,
        CamionService camionService,
        ChauffeurService chauffeurService,
        AchatDto achat)
    {
        InitializeComponent();

        _achatService = achatService;
        _fournisseurService = fournisseurService;
        _produitService = produitService;
        _camionService = camionService;
        _chauffeurService = chauffeurService;
        _originalAchat = achat;

        // Load data and populate fields
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadPickerDataAsync();
        PopulateFields();
    }

    private async Task LoadPickerDataAsync()
    {
        try
        {
            // Load Fournisseurs
            _fournisseurs = await _fournisseurService.GetActiveFournisseursAsync();
            FournisseurPicker.ItemsSource = _fournisseurs;

            // Load Produits
            var allProduits = await _produitService.GetAllProduitsAsync();
            _produits = allProduits
                .Where(p => !string.IsNullOrWhiteSpace(p.Description) || !string.IsNullOrWhiteSpace(p.NumeroProduit))
                .ToList();
            ProduitPicker.ItemsSource = _produits;

            // Load Chauffeurs
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

    private void PopulateFields()
    {
        // Numero (read-only)
        NumeroEntry.Text = _originalAchat.Numero;

        // Date
        AchatDatePicker.Date = _originalAchat.Date.ToDateTime(TimeOnly.MinValue);

        // Fournisseur
        if (_originalAchat.FournisseurID.HasValue)
        {
            var fournisseur = _fournisseurs.FirstOrDefault(f => f.ID == _originalAchat.FournisseurID.Value);
            if (fournisseur != null)
            {
                FournisseurPicker.SelectedItem = fournisseur;
            }
        }

        // Produit
        if (_originalAchat.ProduitID.HasValue)
        {
            var produit = _produits.FirstOrDefault(p => p.ID == _originalAchat.ProduitID.Value);
            if (produit != null)
            {
                ProduitPicker.SelectedItem = produit;
            }
        }

        // Quantité
        if (_originalAchat.Quantite.HasValue)
        {
            QuantiteEntry.Text = _originalAchat.Quantite.Value.ToString();
        }

        // Prix Unitaire
        if (_originalAchat.PrixAchatUnitaire.HasValue)
        {
            PrixUnitaireEntry.Text = _originalAchat.PrixAchatUnitaire.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Chauffeur
        if (_originalAchat.ChauffeurID.HasValue)
        {
            var chauffeur = _chauffeurs.FirstOrDefault(c => c.ID == _originalAchat.ChauffeurID.Value);
            if (chauffeur != null)
            {
                ChauffeurPicker.SelectedItem = chauffeur;
            }
        }

        // Camion
        if (_originalAchat.CamionID.HasValue)
        {
            var camion = _camions.FirstOrDefault(c => c.ID == _originalAchat.CamionID.Value);
            if (camion != null)
            {
                CamionPicker.SelectedItem = camion;
            }
        }

        // Livraison Défectueuse
        LivraisonDefectueuseSwitch.IsToggled = _originalAchat.LivraisonDefectueuse ?? false;

        // Calculate initial total
        CalculateTotalCost();
    }

    private void OnFournisseurChanged(object? sender, EventArgs e)
    {
        if (FournisseurPicker.SelectedItem is FournisseurDto selectedFournisseur)
        {
            // Filter chauffeurs by selected fournisseur
            var filteredChauffeurs = _chauffeurs
                .Where(c => c.FournisseurID == selectedFournisseur.ID || c.FournisseurID == null)
                .ToList();
            ChauffeurPicker.ItemsSource = filteredChauffeurs;

            // Filter camions by fournisseur
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
            // Auto-fill price only if it's a different product
            if (selectedProduit.ID != _originalAchat.ProduitID && selectedProduit.PrixAchat.HasValue)
            {
                PrixUnitaireEntry.Text = selectedProduit.PrixAchat.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
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

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            ErrorLabel.IsVisible = false;

            // Validate required fields
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

            // Calculate total cost
            var coutTotal = quantity * prixUnitaire;

            // Update the achat DTO
            var updatedAchat = new AchatDto
            {
                ID = _originalAchat.ID,
                Numero = _originalAchat.Numero, // Keep original numero
                Date = DateOnly.FromDateTime(AchatDatePicker.Date),
                FournisseurID = selectedFournisseur.ID,
                FournisseurNom = selectedFournisseur.Nom,
                ProduitID = selectedProduit.ID,
                ProduitNom = selectedProduit.Description ?? selectedProduit.NumeroProduit,
                Quantite = quantity,
                PrixAchatUnitaire = prixUnitaire,
                Cout = coutTotal,
                LivraisonDefectueuse = LivraisonDefectueuseSwitch.IsToggled,
                // Preserve audit fields
                AjoutePar = _originalAchat.AjoutePar,
                DateCreation = _originalAchat.DateCreation
            };

            // Set optional fields
            if (ChauffeurPicker.SelectedItem is ChauffeurDto selectedChauffeur)
            {
                updatedAchat.ChauffeurID = selectedChauffeur.ID;
                updatedAchat.ChauffeurNom = selectedChauffeur.Nom;
            }

            if (CamionPicker.SelectedItem is CamionDto selectedCamion)
            {
                updatedAchat.CamionID = selectedCamion.ID;
                updatedAchat.CamionImmatriculation = selectedCamion.Matricule;
            }

            // Save to database via service
            var success = await _achatService.UpdateAsync(updatedAchat);

            if (success)
            {
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la mise à jour de l'achat");
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
        Close(false);
    }
}