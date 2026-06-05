using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the Edit Achat dialog.
/// Pre-populates all fields from the existing <see cref="AchatDto"/> and
/// persists changes via <see cref="AchatService.UpdateAchatAsync"/>.
/// </summary>
public class EditAchatViewModel : ObservableObject
{
    private readonly AchatService _achatService = new();
    private readonly FournisseurService _fournisseurService = new();
    private readonly ProduitService _produitService = new();
    private readonly EmployeService _employeService = new();

    private readonly int _achatId;
    private readonly int? _originalQty;

    // ── Identification ────────────────────────────────────────────────────────

    private string _numero = string.Empty;
    public string Numero
    {
        get => _numero;
        set => SetProperty(ref _numero, value);
    }

    private DateTime _date = DateTime.Today;
    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

    private FournisseurDto? _selectedFournisseur;
    public FournisseurDto? SelectedFournisseur
    {
        get => _selectedFournisseur;
        set => SetProperty(ref _selectedFournisseur, value);
    }

    private ProduitDto? _selectedProduit;
    public ProduitDto? SelectedProduit
    {
        get => _selectedProduit;
        set
        {
            if (SetProperty(ref _selectedProduit, value) && value is not null)
            {
                // Auto-fill unit price from the selected product if the field is empty
                if (PrixAchatUnitaire is null)
                    PrixAchatUnitaire = value.PrixAchat;
                RecalculateCout();
            }
        }
    }

    // ── Financial ─────────────────────────────────────────────────────────────

    private int? _quantite;
    public int? Quantite
    {
        get => _quantite;
        set
        {
            if (SetProperty(ref _quantite, value))
                RecalculateCout();
        }
    }

    private decimal? _prixAchatUnitaire;
    public decimal? PrixAchatUnitaire
    {
        get => _prixAchatUnitaire;
        set
        {
            if (SetProperty(ref _prixAchatUnitaire, value))
                RecalculateCout();
        }
    }

    private decimal? _cout;
    public decimal? Cout
    {
        get => _cout;
        private set => SetProperty(ref _cout, value);
    }

    // ── Delivery ──────────────────────────────────────────────────────────────

    private bool _livraisonDefectueuse;
    public bool LivraisonDefectueuse
    {
        get => _livraisonDefectueuse;
        set => SetProperty(ref _livraisonDefectueuse, value);
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    // ── UI state ──────────────────────────────────────────────────────────────

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

    /// <summary>Set to true after a successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }

    public EditAchatViewModel(AchatDto achat)
    {
        ArgumentNullException.ThrowIfNull(achat);

        _achatId     = achat.AchatId;
        _originalQty = achat.Quantite;

        // Pre-populate without triggering recalculation side-effects
        _numero               = achat.Numero ?? string.Empty;
        _date                 = achat.Date.ToDateTime(TimeOnly.MinValue);
        _quantite             = achat.Quantite;
        _prixAchatUnitaire    = achat.PrixAchatUnitaire;
        _cout                 = achat.Cout;
        _livraisonDefectueuse = achat.LivraisonDefectueuse ?? false;
        _description          = achat.Description;

        // Store IDs so we can pre-select combos after lookups load
        _pendingFournisseurId = achat.FournisseurID;
        _pendingProduitId     = achat.ProduitID;
        _pendingEmployeId     = achat.EmployeId;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    // Pending IDs to resolve once lookup collections are populated
    private readonly int? _pendingFournisseurId;
    private readonly int? _pendingProduitId;
    private readonly int? _pendingEmployeId;

    /// <summary>Loads combo-box data and pre-selects the existing FK values.</summary>
    public async Task LoadLookupsAsync()
    {
        IsLoading = true;
        try
        {
            var fournisseurs = await _fournisseurService.GetAllFournisseursAsync();
            foreach (var f in fournisseurs)
                Fournisseurs.Add(f);

            var produits = await _produitService.GetAllProduitsAsync();
            foreach (var p in produits)
                Produits.Add(p);

            SelectedFournisseur = Fournisseurs.FirstOrDefault(f => f.FournisseurId == _pendingFournisseurId);

            // Pre-select without auto-filling the price again (it's already set)
            _selectedProduit = Produits.FirstOrDefault(p => p.ProduitId == _pendingProduitId);
            OnPropertyChanged(nameof(SelectedProduit));

            var employes = await _employeService.GetAllEmployesAsync();
            foreach (var e in employes)
                Employes.Add(e);
            SelectedEmploye = Employes.FirstOrDefault(e => e.EmployeId == _pendingEmployeId);
        }
        finally { IsLoading = false; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RecalculateCout()
    {
        Cout = (Quantite.HasValue && PrixAchatUnitaire.HasValue)
            ? Math.Round(Quantite.Value * PrixAchatUnitaire.Value, 2)
            : (decimal?)null;
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = LanguageManager.Instance["EditAchatValidationNumero"];
            return;
        }
        if (SelectedFournisseur is null)
        {
            ErrorMessage = LanguageManager.Instance["EditAchatValidationFournisseur"];
            return;
        }
        if (SelectedProduit is null)
        {
            ErrorMessage = LanguageManager.Instance["EditAchatValidationProduit"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new AchatDto
            {
                AchatId              = _achatId,
                Numero               = Numero.Trim(),
                Date                 = DateOnly.FromDateTime(Date),
                FournisseurID        = SelectedFournisseur.FournisseurId,
                ProduitID            = SelectedProduit.ProduitId,
                EmployeId            = SelectedEmploye?.EmployeId,
                Quantite             = Quantite,
                PrixAchatUnitaire    = PrixAchatUnitaire,
                Cout                 = Cout,
                LivraisonDefectueuse = LivraisonDefectueuse,
                Description          = NullIfBlank(Description)
            };

            var ok = await _achatService.UpdateAchatAsync(dto).ConfigureAwait(false);
            if (ok)
            {
                // Adjust product stock by the difference in quantity
                var delta = (Quantite ?? 0) - (_originalQty ?? 0);
                if (delta != 0)
                    await _produitService.UpdateStockAsync(SelectedProduit.ProduitId, delta).ConfigureAwait(false);

                Saved = true;
            }
            else
            {
                ErrorMessage = LanguageManager.Instance["EditAchatSaveError"];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = LanguageManager.Instance["EditAchatSaveError"];
            Debug.WriteLine($"[EditAchatVM] SaveAsync failed: {ex}");
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
