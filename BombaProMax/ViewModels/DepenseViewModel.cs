using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels
{
    /// <summary>
    /// ViewModel for managing expenses (Depenses).
    /// </summary>
    public partial class DepenseViewModel : ObservableObject
    {
        private readonly DepenseService _depenseService;
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
        private DepenseDto? selectedDepense;

        [ObservableProperty]
        private DateTime filterStartDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private DateTime filterEndDate = DateTime.Today;

        [ObservableProperty]
        private string selectedCategory = "Tous";

        #endregion

        #region Collections

        public ObservableCollection<DepenseDto> Depenses { get; } = [];
        public ObservableCollection<string> Categories { get; } = ["Tous"];

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
                LoadDepensesAsync(),
                LoadCategoriesAsync()
            );
        }

        #endregion

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

                NotifyTotalsChanged();
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

                NotifyTotalsChanged();
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

                NotifyTotalsChanged();
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

        private async Task LoadCategoriesAsync()
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

                Debug.WriteLine($"[DepenseViewModel] Loaded {Categories.Count} categories");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseViewModel] Error loading categories: {ex.Message}");
            }
        }

        #endregion

        #region CRUD Operations

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
                    NotifyTotalsChanged();

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
                    NotifyTotalsChanged();

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
                    NotifyTotalsChanged();
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

        #region Helper Methods

        private void NotifyTotalsChanged()
        {
            OnPropertyChanged(nameof(TotalDepenses));
            OnPropertyChanged(nameof(TotalMontant));
            OnPropertyChanged(nameof(MoyenneMontant));
            OnPropertyChanged(nameof(MaxMontant));
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
