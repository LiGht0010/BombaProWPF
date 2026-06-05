using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauAchat creation dialog.
/// </summary>
public class NouveauAchatViewModel : ObservableObject
{
    private readonly AchatService _achatService = new();
    private readonly FournisseurService _fournisseurService = new();
    private readonly ProduitService _produitService = new();
    private readonly EmployeService _employeService = new();

    // ── Form fields ───────────────────────────────────────────────────────────

    private string? _numero;
    public string? Numero
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
                PrixAchatUnitaire = value.PrixAchat;
        }
    }

    private int? _quantite;
    public int? Quantite
    {
        get => _quantite;
        set
        {
            if (SetProperty(ref _quantite, value))
                OnPropertyChanged(nameof(Cout));
        }
    }

    /// <summary>Auto-calculated: PrixAchatUnitaire × Quantite. Read-only in the UI.</summary>
    public decimal? Cout => PrixAchatUnitaire.HasValue && Quantite.HasValue
        ? PrixAchatUnitaire.Value * Quantite.Value
        : (decimal?)null;

    private decimal? _prixAchatUnitaire;
    public decimal? PrixAchatUnitaire
    {
        get => _prixAchatUnitaire;
        set
        {
            if (SetProperty(ref _prixAchatUnitaire, value))
                OnPropertyChanged(nameof(Cout));
        }
    }

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

    // ── Lookup lists ──────────────────────────────────────────────────────────

    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

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

    public IAsyncRelayCommand SaveCommand { get; }

    public NouveauAchatViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    /// <summary>Loads Fournisseurs and Produits combo data.</summary>
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

            var employes = await _employeService.GetAllEmployesAsync();
            foreach (var e in employes)
                Employes.Add(e);
        }
        finally { IsLoading = false; }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = LanguageManager.Instance["NouveauAchatValidationNumero"];
            return;
        }
        if (SelectedFournisseur is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauAchatValidationFournisseur"];
            return;
        }
        if (SelectedProduit is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauAchatValidationProduit"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new AchatDto
            {
                Numero               = Numero.Trim(),
                Date                 = DateOnly.FromDateTime(Date),
                FournisseurID        = SelectedFournisseur.FournisseurId,
                ProduitID            = SelectedProduit.ProduitId,
                EmployeId            = SelectedEmploye?.EmployeId,
                Quantite             = Quantite,
                Cout                 = Cout,
                PrixAchatUnitaire    = PrixAchatUnitaire,
                LivraisonDefectueuse = LivraisonDefectueuse,
                Description          = NullIfBlank(Description)
            };

            var result = await _achatService.CreateAchatAsync(dto);
            if (result is not null)
            {
                // Update the product's stock by the purchased quantity
                if (Quantite.HasValue && Quantite.Value > 0)
                    await _produitService.UpdateStockAsync(SelectedProduit.ProduitId, Quantite.Value);

                Saved = true;
            }
            else
                ErrorMessage = LanguageManager.Instance["NouveauAchatSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
