using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ClientViews;

public partial class CreditTransactionPopup : Popup
{
    private readonly ClientDto _client;
    private readonly CreditTransactionDto? _existingTransaction;
    private readonly List<ProduitDto> _produits;
    private readonly List<ServiceDto> _services;
    private readonly bool _isEditMode;
    private bool _isUpdatingValues; // Prevent recursive updates

    public CreditTransactionPopup(
        ClientDto client,
        List<ProduitDto> produits,
        List<ServiceDto> services,
        CreditTransactionDto? existingTransaction = null,
        string? generatedNumero = null)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;


        _client = client;
        _produits = produits;
        _services = services;
        _existingTransaction = existingTransaction;
        _isEditMode = existingTransaction != null;

        // Set up UI
        ClientNameLabel.Text = client.Nom;
        ProduitPicker.ItemsSource = _produits;
        ServicePicker.ItemsSource = _services;
        DateCreditPicker.Date = DateTime.UtcNow;

        if (_isEditMode && existingTransaction != null)
        {
            HeaderLabel.Text = "✏️ Modifier Transaction Crédit";
            SaveButton.Text = "✓ Mettre à jour";
            PopulateForEdit(existingTransaction);
        }
        else
        {
            NumeroEntry.Text = generatedNumero ?? "";
            QuantiteEntry.Text = "1.00";
        }
    }

    private void PopulateForEdit(CreditTransactionDto transaction)
    {
        NumeroEntry.Text = transaction.NumeroTransaction;
        DateCreditPicker.Date = transaction.DateCredit;
        QuantiteEntry.Text = transaction.Quantite.ToString("F2");
        PrixTTCEntry.Text = transaction.PrixTTC.ToString("F2");
        FactureSwitch.IsToggled = transaction.Facture;

        // Set article type and selection
        if (transaction.ProduitID.HasValue)
        {
            ProduitRadio.IsChecked = true;
            var produit = _produits.FirstOrDefault(p => p.ID == transaction.ProduitID.Value);
            if (produit != null)
            {
                ProduitPicker.SelectedItem = produit;
            }
        }
        else if (transaction.ServiceID.HasValue)
        {
            ServiceRadio.IsChecked = true;
            var service = _services.FirstOrDefault(s => s.ID == transaction.ServiceID.Value);
            if (service != null)
            {
                ServicePicker.SelectedItem = service;
            }
        }

        UpdateMontantTotal();
    }

    private void OnArticleTypeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;

        ProduitSection.IsVisible = ProduitRadio.IsChecked;
        ServiceSection.IsVisible = ServiceRadio.IsChecked;

        // Clear selections when switching
        if (ProduitRadio.IsChecked)
        {
            ServicePicker.SelectedItem = null;
        }
        else
        {
            ProduitPicker.SelectedItem = null;
        }
    }

    private void OnProduitSelected(object? sender, EventArgs e)
    {
        if (ProduitPicker.SelectedItem is ProduitDto produit && produit.PrixTTC.HasValue)
        {
            PrixTTCEntry.Text = produit.PrixTTC.Value.ToString("F2");
            UpdateMontantTotal();
        }
    }

    private void OnServiceSelected(object? sender, EventArgs e)
    {
        if (ServicePicker.SelectedItem is ServiceDto service && service.Prix.HasValue)
        {
            PrixTTCEntry.Text = service.Prix.Value.ToString("F2");
            UpdateMontantTotal();
        }
    }

    private void OnQuantityOrPriceChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingValues) return;

        // If in "calculate by total" mode and price changed, recalculate quantity
        if (CalculateByTotalRadio.IsChecked && sender == PrixTTCEntry)
        {
            if (decimal.TryParse(MontantTotalEntry.Text, out decimal montantTotal) &&
                decimal.TryParse(PrixTTCEntry.Text, out decimal prix) && prix > 0)
            {
                _isUpdatingValues = true;
                var quantite = montantTotal / prix;
                QuantiteEntry.Text = quantite.ToString("F2");
                _isUpdatingValues = false;
            }
        }

        UpdateMontantTotal();
    }

    private void UpdateMontantTotal()
    {
        if (decimal.TryParse(QuantiteEntry.Text, out decimal quantite) &&
            decimal.TryParse(PrixTTCEntry.Text, out decimal prix))
        {
            var total = quantite * prix;
            MontantTotalLabel.Text = $"{total:N2} MAD";
        }
        else
        {
            MontantTotalLabel.Text = "0.00 MAD";
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        // Validation
        if (!ValidateForm())
        {
            return;
        }

        var transaction = _existingTransaction ?? new CreditTransactionDto();

        transaction.ClientID = _client.ID;
        transaction.NumeroTransaction = NumeroEntry.Text;
        transaction.DateCredit = DateCreditPicker.Date;
        transaction.Quantite = decimal.Parse(QuantiteEntry.Text);
        transaction.PrixTTC = decimal.Parse(PrixTTCEntry.Text);
        transaction.MontantTotal = transaction.Quantite * transaction.PrixTTC;
        transaction.Facture = FactureSwitch.IsToggled;

        // Set Produit or Service
        if (ProduitRadio.IsChecked && ProduitPicker.SelectedItem is ProduitDto selectedProduit)
        {
            transaction.ProduitID = selectedProduit.ID;
            transaction.ProduitNom = selectedProduit.Description;
            transaction.ServiceID = null;
            transaction.ServiceNom = null;
        }
        else if (ServiceRadio.IsChecked && ServicePicker.SelectedItem is ServiceDto selectedService)
        {
            transaction.ServiceID = selectedService.ID;
            transaction.ServiceNom = selectedService.Description;
            transaction.ProduitID = null;
            transaction.ProduitNom = null;
        }

        Close(transaction);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        // Check quantity (now decimal)
        if (!decimal.TryParse(QuantiteEntry.Text, out decimal quantite) || quantite <= 0)
        {
            ShowError("Veuillez entrer une quantité valide (> 0)");
            return false;
        }

        // Check price
        if (!decimal.TryParse(PrixTTCEntry.Text, out decimal prix) || prix <= 0)
        {
            ShowError("Veuillez entrer un prix valide (> 0)");
            return false;
        }

        // Check article selection
        if (ProduitRadio.IsChecked && ProduitPicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un produit");
            return false;
        }

        if (ServiceRadio.IsChecked && ServicePicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un service");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnCalculationModeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;

        bool calculateByTotal = CalculateByTotalRadio.IsChecked;

        // Toggle visibility and editability
        MontantEntrySection.IsVisible = calculateByTotal;
        QuantiteEntry.IsReadOnly = calculateByTotal;
        QuantiteEntry.BackgroundColor = calculateByTotal ? Color.FromArgb("#EEEEEE") : Colors.White;
        QuantiteLabel.Text = calculateByTotal ? "📊 Quantité (calculée)" : "📊 Quantité *";

        if (calculateByTotal)
        {
            // When switching to "calculate by total" mode, initialize MontantTotalEntry with current total
            if (decimal.TryParse(QuantiteEntry.Text, out decimal quantite) &&
                decimal.TryParse(PrixTTCEntry.Text, out decimal prix))
            {
                var total = quantite * prix;
                MontantTotalEntry.Text = total.ToString("F2");
            }
        }
    }

    private void OnMontantTotalChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingValues) return;

        // Calculate quantity from total and unit price
        if (decimal.TryParse(MontantTotalEntry.Text, out decimal montantTotal) &&
            decimal.TryParse(PrixTTCEntry.Text, out decimal prix) && prix > 0)
        {
            _isUpdatingValues = true;
            var quantite = montantTotal / prix;
            QuantiteEntry.Text = quantite.ToString("F2");
            MontantTotalLabel.Text = $"{montantTotal:N2} MAD";
            _isUpdatingValues = false;
        }
        else if (decimal.TryParse(MontantTotalEntry.Text, out decimal total))
        {
            MontantTotalLabel.Text = $"{total:N2} MAD";
        }
        else
        {
            MontantTotalLabel.Text = "0.00 MAD";
        }
    }
}