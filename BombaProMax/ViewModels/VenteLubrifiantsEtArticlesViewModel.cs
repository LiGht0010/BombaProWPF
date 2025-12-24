using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels
{
    /// <summary>
    /// ViewModel for managing lubricants and articles sales.
    /// </summary>
    public partial class VenteLubrifiantsEtArticlesViewModel : ObservableObject
    {
        private readonly VenteLubrifiantsEtArticlesService _venteService;
        private readonly ProduitService _produitService;
        private readonly JourneeNavigationService _journeeService;

        #region Observable Properties

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isSaving;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private VenteLubrifiantsEtArticlesDto? selectedVente;

        [ObservableProperty]
        private DateTime filterStartDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime filterEndDate = DateTime.Today;

        [ObservableProperty]
        private string selectedCategory = "Tous";

        #endregion

        #region Collections

        public ObservableCollection<VenteLubrifiantsEtArticlesDto> Ventes { get; } = [];
        public ObservableCollection<ProduitDto> AvailableProduits { get; } = [];
        public ObservableCollection<string> Categories { get; } = ["Tous", "Lubrifiant", "Articles"];

        #endregion

        #region Computed Properties

        /// <summary>
        /// Total sales count.
        /// </summary>
        public int TotalVentes => Ventes.Count;

        /// <summary>
        /// Total quantity sold.
        /// </summary>
        public int TotalQuantite => Ventes.Sum(v => v.QuantiteVendue);

        /// <summary>
        /// Total revenue TTC.
        /// </summary>
        public decimal TotalMontantTTC => Ventes.Sum(v => v.MontantTotalTTC);

        /// <summary>
        /// Total revenue HT.
        /// </summary>
        public decimal TotalMontantHT => Ventes.Sum(v => v.MontantTotalHT);

        /// <summary>
        /// Total TVA.
        /// </summary>
        public decimal TotalTVA => Ventes.Sum(v => v.MontantTVA);

        /// <summary>
        /// Total profit margin.
        /// </summary>
        public decimal TotalMarge => Ventes.Sum(v => v.MargeBeneficiaire ?? 0);

        #endregion

        // ════════════════════════════════════════════════════════════════
        // JOURNÉE PROPERTIES
        // ════════════════════════════════════════════════════════════════
        public bool IsJourneeActive => _journeeService.IsJourneeActive;
        public bool CanGoPrevious => _journeeService.CanGoPrevious;
        public bool CanGoNext => _journeeService.CanGoNext;
        public bool IsFirstStep => _journeeService.IsFirstStep;
        public bool IsLastStep => _journeeService.IsLastStep;
        public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

        #region Constructor

        public VenteLubrifiantsEtArticlesViewModel(JourneeNavigationService journeeService)
        {
            _venteService = new VenteLubrifiantsEtArticlesService();
            _produitService = new ProduitService();
            _journeeService = journeeService;

            _journeeService.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsJourneeActive));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(IsFirstStep));
                OnPropertyChanged(nameof(IsLastStep));
                OnPropertyChanged(nameof(JourneeStepInfo));
            };
        }

        #endregion

        // ════════════════════════════════════════════════════════════════
        // JOURNÉE COMMANDS
        // ════════════════════════════════════════════════════════════════
        [RelayCommand]
        private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

        [RelayCommand]
        private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

        [RelayCommand]
        private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

        // ════════════════════════════════════════════════════════════════
        // EXISTING METHODS
        // ════════════════════════════════════════════════════════════════

        #region Initialization

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                LoadVentesAsync(),
                LoadProduitsAsync()
            );
        }

        #endregion

        #region Load Operations

        [RelayCommand]
        public async Task LoadVentesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var ventes = await _venteService.GetAllAsync();

                Ventes.Clear();
                foreach (var vente in ventes)
                {
                    Ventes.Add(vente);
                }

                NotifyTotalsChanged();
                Debug.WriteLine($"[VenteLubViewModel] Loaded {Ventes.Count} ventes");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de chargement: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error loading ventes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadVentesByDateRangeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var ventes = await _venteService.GetByDateRangeAsync(FilterStartDate, FilterEndDate);

                // Apply category filter if needed
                if (SelectedCategory != "Tous")
                {
                    ventes = ventes.Where(v => 
                        v.CategorieNom.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                Ventes.Clear();
                foreach (var vente in ventes)
                {
                    Ventes.Add(vente);
                }

                NotifyTotalsChanged();
                Debug.WriteLine($"[VenteLubViewModel] Loaded {Ventes.Count} ventes for date range");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de chargement: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error loading ventes by date: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchVentesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var ventes = await _venteService.SearchAsync(SearchText ?? "");

                Ventes.Clear();
                foreach (var vente in ventes)
                {
                    Ventes.Add(vente);
                }

                NotifyTotalsChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de recherche: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error searching ventes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProduitsAsync()
        {
            try
            {
                var produits = await _produitService.GetAllProduitsAsync();
                
                // Filter only non-fuel products (lubricants and articles)
                var nonFuelProduits = produits.Where(p => 
                    p.CategorieNom != null && 
                    !p.CategorieNom.Equals("Carburant", StringComparison.OrdinalIgnoreCase) &&
                    !p.CategorieNom.Equals("Carburants", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                AvailableProduits.Clear();
                foreach (var produit in nonFuelProduits)
                {
                    AvailableProduits.Add(produit);
                }

                Debug.WriteLine($"[VenteLubViewModel] Loaded {AvailableProduits.Count} non-fuel products");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubViewModel] Error loading products: {ex.Message}");
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<VenteLubrifiantsEtArticlesDto?> CreateVenteAsync(VenteLubrifiantsEtArticlesDto vente)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var created = await _venteService.CreateAsync(vente);
                if (created != null)
                {
                    Ventes.Insert(0, created);
                    NotifyTotalsChanged();
                    
                    // Refresh products to get updated stock
                    await LoadProduitsAsync();
                    
                    Debug.WriteLine($"[VenteLubViewModel] Created vente {created.ID}");
                }

                return created;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de création: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error creating vente: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> UpdateVenteAsync(VenteLubrifiantsEtArticlesDto vente)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _venteService.UpdateAsync(vente);
                if (success)
                {
                    // Update in collection
                    var index = Ventes.ToList().FindIndex(v => v.ID == vente.ID);
                    if (index >= 0)
                    {
                        Ventes[index] = vente;
                    }
                    NotifyTotalsChanged();
                    Debug.WriteLine($"[VenteLubViewModel] Updated vente {vente.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error updating vente: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> DeleteVenteAsync(VenteLubrifiantsEtArticlesDto vente)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _venteService.DeleteAsync(vente.ID);
                if (success)
                {
                    Ventes.Remove(vente);
                    if (SelectedVente?.ID == vente.ID)
                    {
                        SelectedVente = null;
                    }
                    NotifyTotalsChanged();
                    
                    // Refresh products to get updated stock
                    await LoadProduitsAsync();
                    
                    Debug.WriteLine($"[VenteLubViewModel] Deleted vente {vente.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de suppression: {ex.Message}";
                Debug.WriteLine($"[VenteLubViewModel] Error deleting vente: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        #endregion

        #region Helper Methods

        private void NotifyTotalsChanged()
        {
            OnPropertyChanged(nameof(TotalVentes));
            OnPropertyChanged(nameof(TotalQuantite));
            OnPropertyChanged(nameof(TotalMontantTTC));
            OnPropertyChanged(nameof(TotalMontantHT));
            OnPropertyChanged(nameof(TotalTVA));
            OnPropertyChanged(nameof(TotalMarge));
        }

        /// <summary>
        /// Clears any error message.
        /// </summary>
        public void ClearError()
        {
            ErrorMessage = null;
        }

        #endregion
    }
}
