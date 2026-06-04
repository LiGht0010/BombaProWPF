using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauVente creation dialog.
/// </summary>
public class NouveauVenteViewModel : ObservableObject
{
    private readonly VenteService   _venteService   = new();
    private readonly ClientService  _clientService  = new();
    private readonly ProduitService _produitService = new();

    // ── Form fields ──────────────────────────────────────────────────────────

    private DateTime _dateVente = DateTime.Today;
    public DateTime DateVente
    {
        get => _dateVente;
        set => SetProperty(ref _dateVente, value);
    }

    private string _numeroVente = GenerateNumero();
    public string NumeroVente
    {
        get => _numeroVente;
        private set => SetProperty(ref _numeroVente, value);
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
            if (SetProperty(ref _selectedProduit, value) && value is not null)
                PrixUnitaire = value.PrixTTC;   // snapshot the current selling price
        }
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

    // ── Lookup lists ─────────────────────────────────────────────────────────

    public ObservableCollection<ClientDto>  Clients  { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];

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

    public IAsyncRelayCommand SaveCommand   { get; }
    public IRelayCommand      BrowseReferenceFileCommand { get; }

    public NouveauVenteViewModel()
    {
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
            var clients  = await _clientService.GetAllClientsAsync();
            foreach (var c in clients)  Clients.Add(c);

            var produits = await _produitService.GetAllProduitsAsync();
            foreach (var p in produits) Produits.Add(p);
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
        MontantTotal = (decimal)Quantite.Value * PrixUnitaire.Value * (1 - remise / 100m);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedClient is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauVenteValidationClient"];
            return;
        }
        if (SelectedProduit is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauVenteValidationProduit"];
            return;
        }
        if (Quantite is null or <= 0)
        {
            ErrorMessage = LanguageManager.Instance["NouveauVenteValidationQuantite"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new VenteDto
            {
                NumeroVente   = NumeroVente,
                DateVente     = DateOnly.FromDateTime(DateVente),
                ClientID      = SelectedClient.ClientId,
                ProduitID     = SelectedProduit.ProduitId,
                Quantite      = Quantite,
                PrixUnitaire  = PrixUnitaire,
                Remise        = Remise,
                PaymentMethod = SelectedPaymentMethod,
                Note          = NullIfBlank(Note),
                Reference     = NullIfBlank(Reference),
                ReferenceFile = NullIfBlank(ReferenceFile),
            };

            var result = await _venteService.CreateVenteAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["NouveauVenteSaveError"];
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
