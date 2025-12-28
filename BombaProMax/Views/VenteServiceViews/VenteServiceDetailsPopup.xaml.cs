using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.VenteServiceViews;

public partial class VenteServiceDetailsPopup : Popup
{
    private readonly VenteServiceDto _vente;
    private bool _requestEdit;

    public VenteServiceDetailsPopup(VenteServiceDto vente)
    {
        InitializeComponent();
        _vente = vente;
        _requestEdit = false;
        PopulateDetails();
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

        // Audit info
        DateCreationLabel.Text = _vente.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        CreeParLabel.Text = _vente.CreePar?.ToString() ?? "-";
        DateModificationLabel.Text = _vente.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        ModifieParLabel.Text = _vente.ModifiePar?.ToString() ?? "-";
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
