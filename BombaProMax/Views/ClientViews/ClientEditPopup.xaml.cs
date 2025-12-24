using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using BombaProMax.Models;
using BombaProMax.Services;


namespace BombaProMax.Views.ClientViews;

public partial class ClientEditPopup : Popup, INotifyPropertyChanged
{
    private readonly ClientService _clientService;
    private readonly ClientDto _clientToEdit;

    private int _facturesCount;
    public int FacturesCount
    {
        get => _facturesCount;
        set
        {
            if (_facturesCount != value)
            {
                _facturesCount = value;
                OnPropertyChanged();
            }
        }
    }

    private int _creditsCount;
    public int CreditsCount
    {
        get => _creditsCount;
        set
        {
            if (_creditsCount != value)
            {
                _creditsCount = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _numeroClient;
    public string? NumeroClient
    {
        get => _numeroClient;
        set
        {
            if (_numeroClient != value)
            {
                _numeroClient = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _nom;
    public string? Nom
    {
        get => _nom;
        set
        {
            if (_nom != value)
            {
                _nom = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _contact;
    public string? Contact
    {
        get => _contact;
        set
        {
            if (_contact != value)
            {
                _contact = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _cin;
    public string? CIN
    {
        get => _cin;
        set
        {
            if (_cin != value)
            {
                _cin = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _nomSociete;
    public string? NomSociete
    {
        get => _nomSociete;
        set
        {
            if (_nomSociete != value)
            {
                _nomSociete = value;
                OnPropertyChanged();
            }
        }
    }

    public ClientEditPopup(ClientService clientService, ClientDto client)
    {
        InitializeComponent();
        _clientService = clientService;
        _clientToEdit = client;

        // Set the binding context to this popup
        BindingContext = this;

        // Load client data
        LoadClientData();
    }

    private async void LoadClientData()
    {
        try
        {
            // Set basic client info for the badge
            ClientBadgeLabel.Text = _clientToEdit.Nom;
            ClientNumberLabel.Text = $"Client #{_clientToEdit.NumeroClient}";

            // Set form fields using properties (for binding)
            NumeroClient = _clientToEdit.NumeroClient;
            Nom = _clientToEdit.Nom;
            Contact = _clientToEdit.Contact;
            CIN = _clientToEdit.CIN;
            NomSociete = _clientToEdit.NomSociete;

            // Load transaction counts
            FacturesCount = 0;
            CreditsCount = 0;

            // Try to get credit balance
            try
            {
                var creditBalance = await _clientService.GetClientCreditBalanceAsync(_clientToEdit.ID);
                if (creditBalance != null && creditBalance.Balance > 0)
                {
                    CreditsCount = 1;
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur de chargement: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Hide any previous errors
            ErrorLabel.IsVisible = false;

            // Validate input
            if (string.IsNullOrWhiteSpace(Nom))
            {
                ShowError("Le nom est requis");
                return;
            }

            if (string.IsNullOrWhiteSpace(CIN))
            {
                ShowError("Le CIN est requis");
                return;
            }

            if (string.IsNullOrWhiteSpace(NomSociete))
            {
                ShowError("Le nom de la société est requis");
                return;
            }

            // Check if CIN already exists (excluding current client)
            var cinExists = await _clientService.ClientCINExistsAsync(CIN.Trim(), _clientToEdit.ID);
            if (cinExists)
            {
                ShowError("Un autre client avec ce CIN existe déjà");
                return;
            }

            // Update DTO properties
            _clientToEdit.NumeroClient = NumeroClient?.Trim() ?? _clientToEdit.NumeroClient;
            _clientToEdit.Nom = Nom.Trim();
            _clientToEdit.Contact = Contact?.Trim();
            _clientToEdit.CIN = CIN.Trim();
            _clientToEdit.NomSociete = NomSociete.Trim();

            // Save to database via service
            var success = await _clientService.UpdateClientAsync(_clientToEdit);

            if (success)
            {
                // Close popup and return success
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la mise à jour du client");
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

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
