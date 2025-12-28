using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteServiceViews;

public partial class VenteServiceCreatePopup : Popup
{
    private readonly VenteServiceDto? _existingVente;
    private readonly bool _isEditMode;
    private readonly List<ServiceDto> _services;
    private readonly List<ServiceCategorieDto> _categories;
    private readonly List<MoyensPaiementDto> _moyensPaiement;

    public VenteServiceCreatePopup(
        List<ServiceDto> services,
        List<ServiceCategorieDto> categories,
        List<MoyensPaiementDto> moyensPaiement,
        VenteServiceDto? existingVente = null)
    {
        InitializeComponent();

        _services = services;
        _categories = categories;
        _moyensPaiement = moyensPaiement;
        _existingVente = existingVente;
        _isEditMode = existingVente != null;

        DatePicker.Date = DateTime.Today;
        LoadServices();
        LoadMoyensPaiement();

        if (_isEditMode && existingVente != null)
        {
            HeaderLabel.Text = "Modifier la Vente";
            SaveButton.Text = "Mettre a jour";
            PopulateForEdit(existingVente);
        }
        else
        {
            NumeroEntry.Text = GenerateNumeroVente();
            // Default to first payment method (usually "Especes")
            if (_moyensPaiement.Count > 0)
            {
                MoyenPaiementPicker.SelectedIndex = 0;
            }
        }

        UpdateSummary();
    }

    private static string GenerateNumeroVente()
    {
        return $"VS-{DateTime.Now:yyyyMMddHHmmss}";
    }

    private void LoadServices()
    {
        var serviceDescriptions = _services.Select(s => s.Description ?? s.Numero ?? "Service sans nom").ToList();
        ServicePicker.ItemsSource = serviceDescriptions;
    }

    private void LoadMoyensPaiement()
    {
        var moyensNoms = _moyensPaiement.Select(m => m.Nom ?? "Inconnu").ToList();
        MoyenPaiementPicker.ItemsSource = moyensNoms;
    }

    private void PopulateForEdit(VenteServiceDto vente)
    {
        DatePicker.Date = vente.DateVente;
        NumeroEntry.Text = vente.NumeroVente;
        QuantiteEntry.Text = vente.Quantite.ToString();
        PrixEntry.Text = vente.PrixUnitaire.ToString("F2");
        NotesEditor.Text = vente.Notes;

        var serviceIndex = _services.FindIndex(s => s.ID == vente.ServiceID);
        if (serviceIndex >= 0)
        {
            ServicePicker.SelectedIndex = serviceIndex;
        }

        // Select the payment method
        if (vente.MoyenPaiementID.HasValue)
        {
            var moyenIndex = _moyensPaiement.FindIndex(m => m.ID == vente.MoyenPaiementID.Value);
            if (moyenIndex >= 0)
            {
                MoyenPaiementPicker.SelectedIndex = moyenIndex;
            }
        }

        UpdateSummary();
    }

    private void OnServiceSelected(object? sender, EventArgs e)
    {
        if (ServicePicker.SelectedIndex >= 0 && ServicePicker.SelectedIndex < _services.Count)
        {
            var selectedService = _services[ServicePicker.SelectedIndex];
            
            if (selectedService.Prix.HasValue)
            {
                PrixEntry.Text = selectedService.Prix.Value.ToString("F2");
            }

            UpdateSummary();
        }
    }

    private void OnQuantityOrPriceChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (int.TryParse(QuantiteEntry.Text, out int quantite) && quantite > 0 &&
            decimal.TryParse(PrixEntry.Text, out decimal prix) && prix >= 0)
        {
            var total = quantite * prix;
            TotalLabel.Text = $"{total:N2} DH";
            SummaryCard.IsVisible = true;
        }
        else
        {
            TotalLabel.Text = "0.00 DH";
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

        ServiceDto? selectedService = null;
        if (ServicePicker.SelectedIndex >= 0 && ServicePicker.SelectedIndex < _services.Count)
        {
            selectedService = _services[ServicePicker.SelectedIndex];
        }

        MoyensPaiementDto? selectedMoyen = null;
        if (MoyenPaiementPicker.SelectedIndex >= 0 && MoyenPaiementPicker.SelectedIndex < _moyensPaiement.Count)
        {
            selectedMoyen = _moyensPaiement[MoyenPaiementPicker.SelectedIndex];
        }

        int.TryParse(QuantiteEntry.Text, out int quantite);
        decimal.TryParse(PrixEntry.Text, out decimal prix);

        // Convert DatePicker.Date to UTC for PostgreSQL compatibility
        var dateVente = DateTime.SpecifyKind(DatePicker.Date, DateTimeKind.Utc);

        var result = new VenteServiceDto
        {
            ID = _existingVente?.ID ?? 0,
            NumeroVente = NumeroEntry.Text,
            DateVente = dateVente,
            ServiceID = selectedService?.ID ?? 0,
            ServiceDescription = selectedService?.Description,
            ServiceCategorieNom = selectedService?.ServiceCategorieNom,
            Quantite = quantite,
            PrixUnitaire = prix,
            MontantTotal = quantite * prix,
            MoyenPaiementID = selectedMoyen?.ID,
            MoyenPaiementNom = selectedMoyen?.Nom,
            Notes = NotesEditor.Text,
            Statut = "Confirmee"
        };

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        if (ServicePicker.SelectedIndex < 0)
        {
            ShowError("Veuillez selectionner un service");
            return false;
        }

        if (!int.TryParse(QuantiteEntry.Text, out int quantite) || quantite <= 0)
        {
            ShowError("Quantite invalide (doit etre > 0)");
            return false;
        }

        if (!decimal.TryParse(PrixEntry.Text, out decimal prix) || prix < 0)
        {
            ShowError("Prix invalide (doit etre >= 0)");
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
