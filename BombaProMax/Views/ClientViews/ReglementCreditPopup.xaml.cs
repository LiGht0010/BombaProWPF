using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ClientViews;

public partial class ReglementCreditPopup : Popup
{
    private readonly ClientDto _client;
    private readonly ReglementCreditDto? _existingReglement;
    private readonly List<MoyensPaiementDto> _moyensPaiement;
    private readonly decimal _currentBalance;
    private readonly bool _isEditMode;

    public ReglementCreditPopup(
        ClientDto client,
        List<MoyensPaiementDto> moyensPaiement,
        decimal currentBalance,
        ReglementCreditDto? existingReglement = null)
    {
        InitializeComponent();

        _client = client;
        _moyensPaiement = moyensPaiement;
        _currentBalance = currentBalance;
        _existingReglement = existingReglement;
        _isEditMode = existingReglement != null;

        // Set up UI
        ClientNameLabel.Text = client.Nom;
        BalanceLabel.Text = $"{currentBalance:N2} MAD";
        BalanceLabel.TextColor = currentBalance > 0 ? Color.FromArgb("#F44336") : Color.FromArgb("#4CAF50");
        ModePaiementPicker.ItemsSource = _moyensPaiement;
        DateReglementPicker.Date = DateTime.Today;

        // Set validator name
        var currentUser = App.CurrentUser ?? App.user;
        ValidateurLabel.Text = currentUser?.Name ?? "Système";

        if (_isEditMode && existingReglement != null)
        {
            HeaderLabel.Text = "?? Modifier Règlement";
            SaveButton.Text = "? Mettre à jour";
            PopulateForEdit(existingReglement);
        }
    }

    private void PopulateForEdit(ReglementCreditDto reglement)
    {
        DateReglementPicker.Date = reglement.DateReglement.Date;
        MontantEntry.Text = reglement.MontantPaye.ToString("F2");
        ReferenceEntry.Text = reglement.ReferenceTransaction;
        RemarquesEditor.Text = reglement.Remarques;
        ValidateurLabel.Text = reglement.ValidePar ?? ValidateurLabel.Text;

        // Set payment mode
        var moyen = _moyensPaiement.FirstOrDefault(m => m.ID == reglement.ModePaiementID);
        if (moyen != null)
        {
            ModePaiementPicker.SelectedItem = moyen;
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

        var reglement = _existingReglement ?? new ReglementCreditDto();

        reglement.ClientID = _client.ID;
        
        // Convert DatePicker date to UTC for PostgreSQL
        var selectedDate = DateReglementPicker.Date;
        reglement.DateReglement = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
        
        reglement.MontantPaye = decimal.Parse(MontantEntry.Text);
        reglement.ReferenceTransaction = ReferenceEntry.Text;
        reglement.Remarques = RemarquesEditor.Text;

        // Set payment mode
        if (ModePaiementPicker.SelectedItem is MoyensPaiementDto selectedMode)
        {
            reglement.ModePaiementID = selectedMode.ID;
            reglement.ModePaiementNom = selectedMode.Nom;
        }

        // Validator is set by the service

        Close(reglement);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        // Check amount
        if (!decimal.TryParse(MontantEntry.Text, out decimal montant) || montant <= 0)
        {
            ShowError("Veuillez entrer un montant valide (> 0)");
            return false;
        }

        // Check payment mode
        if (ModePaiementPicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un mode de paiement");
            return false;
        }

        // Warning if payment exceeds balance (but allow it)
        if (montant > _currentBalance && _currentBalance > 0)
        {
            // This is just informational - overpayment might be intentional (advance payment)
            // You could add a confirmation dialog here if needed
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}