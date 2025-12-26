using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels
{
    /// <summary>
    /// ViewModel for managing expenses (Depenses) and their categories.
    /// </summary>
    public partial class DepenseViewModel : ObservableObject
    {
        private readonly DepenseService _depenseService;
        private readonly DepenseCategorieService _categorieService;
        private readonly JourneeNavigationService _journeeService;

        #region Observable Properties

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private DepenseDto? _selectedDepense;

        [ObservableProperty]
        private DateTime _filterStartDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private DateTime _filterEndDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedCategory = "Tous";

        // Category management properties
        [ObservableProperty]
        private bool _isCategoriesLoading;

        [ObservableProperty]
        private DepenseCategorieDto? _selectedCategorieItem;

        [ObservableProperty]
        private bool _showInactiveCategories;

        #endregion

        #region Collections

        public ObservableCollection<DepenseDto> Depenses { get; } = [];
        public ObservableCollection<string> Categories { get; } = ["Tous"];
        public ObservableCollection<DepenseCategorieDto> CategoriesList { get; } = [];

        #endregion

        #region Computed Properties

        /// <summary>
        /// Total expense count.
        /// </summary>
        public int TotalDepenses => Depenses.Count;

        /// <summary>
        /// Total amount of all expenses.
        /// </summary>
        public decimal TotalMontant => Depenses.Sum(d => d.Montant ?? 0);

        /// <summary>
        /// Average expense amount.
        /// </summary>
        public decimal MoyenneMontant => Depenses.Count > 0 ? TotalMontant / Depenses.Count : 0;

        /// <summary>
        /// Maximum expense amount.
        /// </summary>
        public decimal MaxMontant => Depenses.Count > 0 ? Depenses.Max(d => d.Montant ?? 0) : 0;

        /// <summary>
        /// Total categories count.
        /// </summary>
        public int TotalCategories => CategoriesList.Count;

        /// <summary>
        /// Active categories count.
        /// </summary>
        public int ActiveCategories => CategoriesList.Count(c => c.IsActive);

        /// <summary>
        /// Inactive categories count.
        /// </summary>
        public int InactiveCategories => CategoriesList.Count(c => !c.IsActive);

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

        public DepenseViewModel(JourneeNavigationService journeeService)
        {
            _depenseService = new DepenseService();
            _categorieService = new DepenseCategorieService();
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
        // INITIALIZATION
        // ════════════════════════════════════════════════════════════════

        #region Initialization

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                LoadDepensesAsync(),
                LoadCategoriesAsync(),
                LoadCategoriesListAsync()
            );
        }

        #endregion

        // ════════════════════════════════════════════════════════════════
        // DEPENSE OPERATIONS
        // ════════════════════════════════════════════════════════════════

        #region Load Operations

        [RelayCommand]
        public async Task LoadDepensesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var depenses = await _depenseService.GetAllAsync();

                Depenses.Clear();
                foreach (var depense in depenses)
                {
                    Depenses.Add(depense);
                }

                NotifyDepenseTotalsChanged();
                Debug.WriteLine($"[DepenseViewModel] Loaded {Depenses.Count} depenses");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de chargement: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error loading depenses: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadDepensesByFilterAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var startDate = DateOnly.FromDateTime(FilterStartDate);
                var endDate = DateOnly.FromDateTime(FilterEndDate);

                var depenses = await _depenseService.GetByDateRangeAsync(startDate, endDate);

                // Apply category filter if needed
                if (SelectedCategory != "Tous")
                {
                    depenses = depenses.Where(d =>
                        d.Categorie?.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();
                }

                Depenses.Clear();
                foreach (var depense in depenses)
                {
                    Depenses.Add(depense);
                }

                NotifyDepenseTotalsChanged();
                Debug.WriteLine($"[DepenseViewModel] Loaded {Depenses.Count} depenses for filter");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de chargement: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error loading depenses by filter: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchDepensesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var depenses = await _depenseService.SearchAsync(SearchText ?? "");

                Depenses.Clear();
                foreach (var depense in depenses)
                {
                    Depenses.Add(depense);
                }

                NotifyDepenseTotalsChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de recherche: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error searching depenses: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _depenseService.GetCategoriesAsync();

                Categories.Clear();
                Categories.Add("Tous");
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                Debug.WriteLine($"[DepenseViewModel] Loaded {Categories.Count} categories for filter");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseViewModel] Error loading categories: {ex.Message}");
            }
        }

        #endregion

        #region Depense CRUD Operations

        public async Task<DepenseDto?> CreateDepenseAsync(DepenseDto depense)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var created = await _depenseService.CreateAsync(depense);
                if (created != null)
                {
                    Depenses.Insert(0, created);
                    NotifyDepenseTotalsChanged();

                    // Refresh categories in case a new one was added
                    await LoadCategoriesAsync();

                    Debug.WriteLine($"[DepenseViewModel] Created depense {created.ID}");
                }

                return created;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de création: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error creating depense: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> UpdateDepenseAsync(DepenseDto depense)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _depenseService.UpdateAsync(depense);
                if (success)
                {
                    // Update in collection
                    var index = Depenses.ToList().FindIndex(d => d.ID == depense.ID);
                    if (index >= 0)
                    {
                        Depenses[index] = depense;
                    }
                    NotifyDepenseTotalsChanged();

                    // Refresh categories in case one was changed
                    await LoadCategoriesAsync();

                    Debug.WriteLine($"[DepenseViewModel] Updated depense {depense.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error updating depense: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> DeleteDepenseAsync(DepenseDto depense)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _depenseService.DeleteAsync(depense.ID);
                if (success)
                {
                    Depenses.Remove(depense);
                    if (SelectedDepense?.ID == depense.ID)
                    {
                        SelectedDepense = null;
                    }
                    NotifyDepenseTotalsChanged();
                    Debug.WriteLine($"[DepenseViewModel] Deleted depense {depense.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de suppression: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error deleting depense: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════
        // CATEGORY OPERATIONS
        // ════════════════════════════════════════════════════════════════

        #region Category Load Operations

        [RelayCommand]
        public async Task LoadCategoriesListAsync()
        {
            try
            {
                IsCategoriesLoading = true;
                ErrorMessage = null;

                var categories = ShowInactiveCategories
                    ? await _categorieService.GetAllIncludingInactiveAsync()
                    : await _categorieService.GetAllAsync();

                CategoriesList.Clear();
                foreach (var categorie in categories)
                {
                    CategoriesList.Add(categorie);
                }

                NotifyCategoriesTotalsChanged();
                Debug.WriteLine($"[DepenseViewModel] Loaded {CategoriesList.Count} categories");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de chargement: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error loading categories list: {ex.Message}");
            }
            finally
            {
                IsCategoriesLoading = false;
            }
        }

        #endregion

        #region Category CRUD Operations

        public async Task<DepenseCategorieDto?> CreateCategorieAsync(DepenseCategorieDto categorie)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var created = await _categorieService.CreateAsync(categorie);
                if (created != null)
                {
                    CategoriesList.Insert(0, created);
                    NotifyCategoriesTotalsChanged();

                    // Refresh the filter categories as well
                    await LoadCategoriesAsync();

                    Debug.WriteLine($"[DepenseViewModel] Created category {created.ID}");
                }

                return created;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de création: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error creating category: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> UpdateCategorieAsync(DepenseCategorieDto categorie)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _categorieService.UpdateAsync(categorie);
                if (success)
                {
                    var index = CategoriesList.ToList().FindIndex(c => c.ID == categorie.ID);
                    if (index >= 0)
                    {
                        CategoriesList[index] = categorie;
                    }
                    NotifyCategoriesTotalsChanged();

                    // Refresh the filter categories as well
                    await LoadCategoriesAsync();

                    Debug.WriteLine($"[DepenseViewModel] Updated category {categorie.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error updating category: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        public async Task<bool> DeleteCategorieAsync(DepenseCategorieDto categorie)
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var success = await _categorieService.DeleteAsync(categorie.ID);
                if (success)
                {
                    if (!ShowInactiveCategories)
                    {
                        CategoriesList.Remove(categorie);
                    }
                    else
                    {
                        var index = CategoriesList.ToList().FindIndex(c => c.ID == categorie.ID);
                        if (index >= 0)
                        {
                            CategoriesList[index].IsActive = false;
                        }
                    }

                    if (SelectedCategorieItem?.ID == categorie.ID)
                    {
                        SelectedCategorieItem = null;
                    }
                    NotifyCategoriesTotalsChanged();

                    // Refresh the filter categories as well
                    await LoadCategoriesAsync();

                    Debug.WriteLine($"[DepenseViewModel] Deleted category {categorie.ID}");
                }

                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de suppression: {ex.Message}";
                Debug.WriteLine($"[DepenseViewModel] Error deleting category: {ex.Message}");
                throw;
            }
            finally
            {
                IsSaving = false;
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ════════════════════════════════════════════════════════════════

        #region Helper Methods

        private void NotifyDepenseTotalsChanged()
        {
            OnPropertyChanged(nameof(TotalDepenses));
            OnPropertyChanged(nameof(TotalMontant));
            OnPropertyChanged(nameof(MoyenneMontant));
            OnPropertyChanged(nameof(MaxMontant));
        }

        private void NotifyCategoriesTotalsChanged()
        {
            OnPropertyChanged(nameof(TotalCategories));
            OnPropertyChanged(nameof(ActiveCategories));
            OnPropertyChanged(nameof(InactiveCategories));
        }

        /// <summary>
        /// Clears any error message.
        /// </summary>
        public void ClearError()
        {
            ErrorMessage = null;
        }

        partial void OnShowInactiveCategoriesChanged(bool value)
        {
            _ = LoadCategoriesListAsync();
        }

        #endregion
    }
}
