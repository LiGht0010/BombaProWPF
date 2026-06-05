using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

/// <summary>Edit-dialog ViewModel for an existing Vente.</summary>
public class EditVenteViewModel : ObservableObject
{
    private readonly VenteService   _venteService   = new();
    private readonly ClientService  _clientService  = new();
    private readonly ProduitService _produitService = new();
    private readonly EmployeService _employeService = new();

    private readonly int  _venteId;
    private readonly int? _originalQty;
    private readonly int?      _originalAjoutePar;
    private readonly DateTime? _originalDateCreation;

    // FK ids to restore after lookups load
    private readonly int? _pendingClientId;
    private readonly int? _pendingProduitId;
    private readonly int? _pendingEmployeId;

    // ── Fields ───────────────────────────────────────────────────────────────

    private string _numeroVente = string.Empty;
    public string NumeroVente
    {
        get => _numeroVente;
        set => SetProperty(ref _numeroVente, value);
    }

    private DateTime _dateVente;
    public DateTime DateVente
    {
        get => _dateVente;
        set => SetProperty(ref _dateVente, value);
    }

    private ClientDto? _selectedClient;
    public ClientDto? SelectedClient
    {
        get => _selectedClient;
        set => SetProperty(ref _selectedClient, value);
    }

    private ProduitDto? _selectedProduit;
    public ProduitDto? SelectedProduit
    {
        get => _selectedProduit;
        set
        {
            // In edit mode we do NOT auto-overwrite the snapshotted price
            SetProperty(ref _selectedProduit, value);
        }
    }

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

    private int? _quantite;
    public int? Quantite
    {
        get => _quantite;
        set { if (SetProperty(ref _quantite, value)) RecomputeTotal(); }
    }

    private decimal? _prixUnitaire;
    public decimal? PrixUnitaire
    {
        get => _prixUnitaire;
        set { if (SetProperty(ref _prixUnitaire, value)) RecomputeTotal(); }
    }

    private decimal? _remise;
    public decimal? Remise
    {
        get => _remise;
        set { if (SetProperty(ref _remise, value)) RecomputeTotal(); }
    }

    private decimal? _montantTotal;
    public decimal? MontantTotal
    {
        get => _montantTotal;
        private set => SetProperty(ref _montantTotal, value);
    }

    private string? _selectedPaymentMethod;
    public string? SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    private string? _note;
    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
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

    // ── Lookups ──────────────────────────────────────────────────────────────

    public ObservableCollection<ClientDto>  Clients  { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];

    public IReadOnlyList<string> PaymentMethods { get; } =
        ["TPE", "Virement", "Especes"];

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

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

    public EditVenteViewModel(VenteDto vente)
    {
        _venteId              = vente.VenteId;
        _originalQty          = vente.Quantite;
        _originalAjoutePar    = vente.AjoutePar;
        _originalDateCreation = vente.DateCreation;
        _pendingClientId   = vente.ClientID;
        _pendingProduitId  = vente.ProduitID;
        _pendingEmployeId  = vente.EmployeId;

        // Pre-fill all fields from the existing DTO
        NumeroVente           = string.IsNullOrWhiteSpace(vente.NumeroVente)
                                    ? GenerateNumero()
                                    : vente.NumeroVente;
        DateVente             = vente.DateVente.ToDateTime(TimeOnly.MinValue);
        Quantite              = vente.Quantite;
        PrixUnitaire          = vente.PrixUnitaire;
        Remise                = vente.Remise;
        MontantTotal          = vente.MontantTotal;
        SelectedPaymentMethod = vente.PaymentMethod;
        Note                  = vente.Note;
        Reference             = vente.Reference;
        ReferenceFile         = vente.ReferenceFile;

        SaveCommand                = new AsyncRelayCommand(SaveAsync);
        BrowseReferenceFileCommand = new RelayCommand(BrowseReferenceFile);
    }

    // ── File picker ──────────────────────────────────────────────────────────

    private void BrowseReferenceFile()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Sélectionner le fichier de référence",
            Filter = "Tous les fichiers (*.*)|*.*|Images (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|PDF (*.pdf)|*.pdf"
        };
        if (dlg.ShowDialog() == true)
            ReferenceFile = dlg.FileName;
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task LoadLookupsAsync()
    {
        IsLoading = true;
        try
        {
            var clients = await _clientService.GetAllClientsAsync();
            foreach (var c in clients) Clients.Add(c);

            var produits = await _produitService.GetAllProduitsAsync();
            foreach (var p in produits) Produits.Add(p);

            var employes = await _employeService.GetAllEmployesAsync();
            foreach (var e in employes) Employes.Add(e);

            // Restore FK selections without touching the snapshotted price
            SelectedClient = Clients.FirstOrDefault(c => c.ClientId == _pendingClientId);
            _selectedProduit = Produits.FirstOrDefault(p => p.ProduitId == _pendingProduitId);
            OnPropertyChanged(nameof(SelectedProduit));
            SelectedEmploye = Employes.FirstOrDefault(e => e.EmployeId == _pendingEmployeId);
        }
        finally { IsLoading = false; }
    }

    // ── Computation ──────────────────────────────────────────────────────────

    private void RecomputeTotal()
    {
        if (Quantite is null || PrixUnitaire is null)
        {
            MontantTotal = null;
            return;
        }
        var remise = Remise ?? 0m;
        MontantTotal = Math.Round((decimal)Quantite.Value * PrixUnitaire.Value * (1 - remise / 100m), 2);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedClient is null)
        {
            ErrorMessage = LanguageManager.Instance["EditVenteValidationClient"];
            return;
        }
        if (SelectedProduit is null)
        {
            ErrorMessage = LanguageManager.Instance["EditVenteValidationProduit"];
            return;
        }
        if (Quantite is null or <= 0)
        {
            ErrorMessage = LanguageManager.Instance["EditVenteValidationQuantite"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new VenteDto
            {
                VenteId       = _venteId,
                NumeroVente   = NumeroVente,
                DateVente     = DateOnly.FromDateTime(DateVente),
                ClientID      = SelectedClient.ClientId,
                ProduitID     = SelectedProduit.ProduitId,
                EmployeId     = SelectedEmploye?.EmployeId,
                Quantite      = Quantite,
                PrixUnitaire  = PrixUnitaire,
                Remise        = Remise,
                PaymentMethod = SelectedPaymentMethod,
                Note          = NullIfBlank(Note),
                Reference     = NullIfBlank(Reference),
                ReferenceFile = NullIfBlank(ReferenceFile),
                // Preserve original audit creation fields — the service will stamp ModifiePar/DateModification
                AjoutePar    = _originalAjoutePar,
                DateCreation = _originalDateCreation,
            };

            var ok = await _venteService.UpdateVenteAsync(dto);
            if (ok)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["EditVenteSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string GenerateNumero()
    {
        var n = DateTime.Now;
        return $"V-{n:yyyy-MM-dd-HH-mm-ss}";
    }
}
