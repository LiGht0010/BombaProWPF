using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteServiceViews;

public partial class VenteServiceDetailsPopup : Popup
{
    private readonly VenteServiceDto _vente;
    private readonly UserService _userService;
    private bool _requestEdit;

    public VenteServiceDetailsPopup(VenteServiceDto vente)
    {
        InitializeComponent();
        _vente = vente;
        _userService = new UserService();
        _requestEdit = false;
        PopulateDetails();
        LoadAuditUserNamesAsync();
    }

    private void PopulateDetails()
    {
        // Header info
        NumeroLabel.Text = _vente.NumeroVente ?? "N/A";
        DateLabel.Text = _vente.DateVente.ToString("dd/MM/yyyy");

        // Service info
        ServiceDescriptionLabel.Text = _vente.ServiceDescription ?? _vente.ServiceNumero ?? "N/A";
        ServiceCategorieLabel.Text = _vente.ServiceCategorieNom ?? "Non categorise";

        // Financial info
        QuantiteLabel.Text = _vente.Quantite.ToString();
        PrixUnitaireLabel.Text = $"{_vente.PrixUnitaire:N2} DH";
        MontantTotalLabel.Text = $"{_vente.MontantTotal:N2} DH";

        // Payment info
        MoyenPaiementLabel.Text = _vente.MoyenPaiementNom ?? "Especes";

        // Client info
        ClientNomLabel.Text = string.IsNullOrEmpty(_vente.ClientNom) ? "Comptant" : _vente.ClientNom;

        // Status and employee
        StatutLabel.Text = _vente.Statut ?? "Confirmee";
        EmployeLabel.Text = _vente.EmployeNom ?? "-";

        // Notes
        if (!string.IsNullOrWhiteSpace(_vente.Notes))
        {
            NotesCard.IsVisible = true;
            NotesLabel.Text = _vente.Notes;
        }
        else
        {
            NotesCard.IsVisible = false;
        }

        // Audit info - dates
        DateCreationLabel.Text = _vente.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        DateModificationLabel.Text = _vente.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        
        // Set loading state for user names
        CreeParLabel.Text = "Chargement...";
        ModifieParLabel.Text = "Chargement...";
    }

    private async void LoadAuditUserNamesAsync()
    {
        try
        {
            var createdByName = await _userService.GetUserNameByIdAsync(_vente.CreePar);
            CreeParLabel.Text = createdByName;

            var modifiedByName = await _userService.GetUserNameByIdAsync(_vente.ModifiePar);
            ModifieParLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VenteServiceDetailsPopup] Error loading audit info: {ex.Message}");
            CreeParLabel.Text = "Erreur de chargement";
            ModifieParLabel.Text = "Erreur de chargement";
        }
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        _requestEdit = true;
        Close(_vente);
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    public bool RequestedEdit => _requestEdit;
}
