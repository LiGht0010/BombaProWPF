using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Automation;
using FourniPro.Automation.Credit;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

public class NouveauPaiementCreditViewModel : ObservableObject
{
    private readonly PaiementCreditService _service          = new();
    private readonly CreditService         _creditService    = new();
    private readonly EmployeService        _employeService   = new();
    private readonly AutomationRunner      _automationRunner = new();

    // ── Form fields ──────────────────────────────────────────────────────────

    private CreditDto? _selectedCredit;
    public CreditDto? SelectedCredit
    {
        get => _selectedCredit;
        set => SetProperty(ref _selectedCredit, value);
    }

    private DateTime _datePaiement = DateTime.Today;
    public DateTime DatePaiement
    {
        get => _datePaiement;
        set => SetProperty(ref _datePaiement, value);
    }

    private decimal? _montant;
    public decimal? Montant
    {
        get => _montant;
        set => SetProperty(ref _montant, value);
    }

    private string? _selectedPaymentMethod;
    public string? SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    private string? _reference;
    public string? Reference
    {
        get => _reference;
        set => SetProperty(ref _reference, value);
    }

    private string? _referenceFile;
    public string? ReferenceFile
    {
        get => _referenceFile;
        set => SetProperty(ref _referenceFile, value);
    }

    private string? _note;
    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

    // ── Lookup lists ─────────────────────────────────────────────────────────

    public ObservableCollection<CreditDto>  Credits  { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];

    public IReadOnlyList<string> PaymentMethods { get; } =
        ["Especes", "Virement", "TPE", "ChequeEncaisse"];

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand               { get; }
    public IRelayCommand      BrowseReferenceFileCommand { get; }

    public NouveauPaiementCreditViewModel()
    {
        SaveCommand                = new AsyncRelayCommand(SaveAsync);
        BrowseReferenceFileCommand = new RelayCommand(BrowseReferenceFile);
        _selectedPaymentMethod     = PaymentMethods[0];
    }

    // ── File picker ───────────────────────────────────────────────────────────

    private void BrowseReferenceFile()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Sélectionner le fichier de référence",
            Filter = "Tous les fichiers (*.*)|*.*|Images (*.jpg;*.png)|*.jpg;*.png|PDF (*.pdf)|*.pdf"
        };
        if (dlg.ShowDialog() == true)
            ReferenceFile = dlg.FileName;
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task LoadLookupsAsync()
    {
        var credits  = await _creditService.GetAllCreditsAsync();
        foreach (var c in credits)  Credits.Add(c);

        var employes = await _employeService.GetAllEmployesAsync();
        foreach (var e in employes) Employes.Add(e);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedCredit is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauPaiementValidationCredit"];
            return;
        }
        if (Montant is null or <= 0)
        {
            ErrorMessage = LanguageManager.Instance["NouveauPaiementValidationMontant"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new PaiementCreditDto
            {
                CreditId        = SelectedCredit.CreditId,
                DatePaiement    = DateOnly.FromDateTime(DatePaiement),
                Montant         = Montant,
                PaymentMethod   = SelectedPaymentMethod,
                Reference       = NullIfBlank(Reference),
                ReferenceFile   = NullIfBlank(ReferenceFile),
                Note            = NullIfBlank(Note),
                EmployeId       = SelectedEmploye?.EmployeId,
            };

            var result = await _service.CreateAsync(dto);
            if (result is not null)
            {
                await _automationRunner.RunAsync(
                    AutomationTrigger.PaiementCreditSaved,
                    new PaiementCreditSavedContext(
                        CreditId:     SelectedCredit.CreditId,
                        ClientId:     SelectedCredit.ClientID ?? 0,
                        MontantTotal: SelectedCredit.MontantTotal ?? 0m));
                Saved = true;
            }
            else
                ErrorMessage = LanguageManager.Instance["NouveauPaiementSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
