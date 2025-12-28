using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteServiceViews;

public partial class VenteServiceEditPopup : Popup
{
    private readonly VenteServiceDto _existingVente;
    private readonly List<ServiceDto> _services;
    private readonly List<ServiceCategorieDto> _categories;
    private readonly List<MoyensPaiementDto> _moyensPaiement;
    private readonly List<ClientDto> _clients;

    public VenteServiceEditPopup(
        VenteServiceDto existingVente,
        List<ServiceDto> services,
        List<ServiceCategorieDto> categories,
        List<MoyensPaiementDto> moyensPaiement,
        List<ClientDto> clients)
    {
        InitializeComponent();

        _existingVente = existingVente;
        _services = services;
        _categories = categories;
        _moyensPaiement = moyensPaiement;
        _clients = clients;

        LoadPickers();
        PopulateForEdit();
        UpdateSummary();
    }

    private void LoadPickers()
    {
        // Load services
        var serviceDescriptions = _services.Select(s => s.Description ?? s.Numero ?? "Service sans nom").ToList();
        ServicePicker.ItemsSource = serviceDescriptions;

        // Load payment methods
        var moyensNoms = _moyensPaiement.Select(m => m.Nom ?? "Inconnu").ToList();
        MoyenPaiementPicker.ItemsSource = moyensNoms;

        // Load clients with "Comptant" option first
        var clientNames = new List<string> { "Comptant (sans client)" };
        clientNames.AddRange(_clients.Select(c => c.Nom ?? c.NumeroClient ?? "Client"));
        ClientPicker.ItemsSource = clientNames;
    }

    private void PopulateForEdit()
    {
        // Header
        NumeroLabel.Text = _existingVente.NumeroVente ?? "N/A";
        NumeroEntry.Text = _existingVente.NumeroVente;

        // Date
        DatePicker.Date = _existingVente.DateVente;

        // Service
        var serviceIndex = _services.FindIndex(s => s.ID == _existingVente.ServiceID);
        if (serviceIndex >= 0)
        {
            ServicePicker.SelectedIndex = serviceIndex;
        }

        // Quantity and price
        QuantiteEntry.Text = _existingVente.Quantite.ToString();
        PrixEntry.Text = _existingVente.PrixUnitaire.ToString("F2");

        // Payment method
        if (_existingVente.MoyenPaiementID.HasValue)
        {
            var moyenIndex = _moyensPaiement.FindIndex(m => m.ID == _existingVente.MoyenPaiementID.Value);
            if (moyenIndex >= 0)
            {
                MoyenPaiementPicker.SelectedIndex = moyenIndex;
            }
        }
        else if (_moyensPaiement.Count > 0)
        {
            MoyenPaiementPicker.SelectedIndex = 0;
        }

        // Client
        if (_existingVente.ClientID.HasValue)
        {
            var clientIndex = _clients.FindIndex(c => c.ID == _existingVente.ClientID.Value);
            if (clientIndex >= 0)
            {
                ClientPicker.SelectedIndex = clientIndex + 1; // +1 because of "Comptant" option
            }
            else
            {
                ClientPicker.SelectedIndex = 0;
            }
        }
        else
        {
            ClientPicker.SelectedIndex = 0;
        }

        // Status
        var statuts = new[] { "Confirmee", "En attente", "Annulee" };
        var statutIndex = Array.FindIndex(statuts, s => 
            s.Equals(_existingVente.Statut, StringComparison.OrdinalIgnoreCase));
        StatutPicker.SelectedIndex = statutIndex >= 0 ? statutIndex : 0;

        // Notes
        NotesEditor.Text = _existingVente.Notes;

        // Audit info
        DateCreationLabel.Text = _existingVente.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        CreeParLabel.Text = _existingVente.CreePar?.ToString() ?? "-";
        DateModificationLabel.Text = _existingVente.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        ModifieParLabel.Text = _existingVente.ModifiePar?.ToString() ?? "-";
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

        ClientDto? selectedClient = null;
        if (ClientPicker.SelectedIndex > 0) // Index 0 is "Comptant"
        {
            var clientIndex = ClientPicker.SelectedIndex - 1;
            if (clientIndex >= 0 && clientIndex < _clients.Count)
            {
                selectedClient = _clients[clientIndex];
            }
        }

        int.TryParse(QuantiteEntry.Text, out int quantite);
        decimal.TryParse(PrixEntry.Text, out decimal prix);

        // Convert DatePicker.Date to UTC for PostgreSQL compatibility
        var dateVente = DateTime.SpecifyKind(DatePicker.Date, DateTimeKind.Utc);

        var result = new VenteServiceDto
        {
            ID = _existingVente.ID,
            NumeroVente = _existingVente.NumeroVente,
            DateVente = dateVente,
            ServiceID = selectedService?.ID ?? _existingVente.ServiceID,
            ServiceDescription = selectedService?.Description,
            ServiceCategorieNom = selectedService?.ServiceCategorieNom,
            Quantite = quantite,
            PrixUnitaire = prix,
            MontantTotal = quantite * prix,
            MoyenPaiementID = selectedMoyen?.ID,
            MoyenPaiementNom = selectedMoyen?.Nom,
            ClientID = selectedClient?.ID,
            ClientNom = selectedClient?.Nom,
            Notes = NotesEditor.Text,
            Statut = StatutPicker.SelectedItem?.ToString() ?? "Confirmee",
            // Preserve original audit info
            DateCreation = _existingVente.DateCreation,
            CreePar = _existingVente.CreePar
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
