using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DepenseViews;

public partial class DepenseCreatePopup : Popup
{
    private readonly DepenseDto? _existingDepense;
    private readonly bool _isEditMode;
    private readonly DepenseCategorieService _categorieService;

    public DepenseCreatePopup(DepenseDto? existingDepense = null)
    {
        InitializeComponent();

        _existingDepense = existingDepense;
        _isEditMode = existingDepense != null;
        _categorieService = new DepenseCategorieService();

        // Set default date
        DatePicker.Date = DateTime.Today;

        // Load categories asynchronously
        _ = LoadCategoriesAsync();

        if (_isEditMode && existingDepense != null)
        {
            HeaderLabel.Text = "?? Modifier la Dépense";
            SaveButton.Text = "? Mettre à jour";
            PopulateForEdit(existingDepense);
        }

        // Handle montant changes to show summary
        MontantEntry.TextChanged += OnMontantChanged;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categorieService.GetCategoryNamesAsync();
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CategoriePicker.ItemsSource = categories;
                
                // If editing, try to set the category after loading
                if (_isEditMode && _existingDepense != null && !string.IsNullOrEmpty(_existingDepense.Categorie))
                {
                    if (categories.Contains(_existingDepense.Categorie))
                    {
                        CategoriePicker.SelectedItem = _existingDepense.Categorie;
                    }
                    else
                    {
                        CustomCategorieEntry.Text = _existingDepense.Categorie;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DepenseCreatePopup] Error loading categories: {ex.Message}");
            // Fallback to default categories
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CategoriePicker.ItemsSource = DepenseCategorieService.GetDefaultCategories();
            });
        }
    }

    private void PopulateForEdit(DepenseDto depense)
    {
        if (depense.Date.HasValue)
        {
            DatePicker.Date = depense.Date.Value.ToDateTime(TimeOnly.MinValue);
        }
        
        NumeroEntry.Text = depense.Numero;
        MontantEntry.Text = depense.Montant?.ToString("F2");
        DescriptionEditor.Text = depense.Description;

        // Category will be set after async load completes in LoadCategoriesAsync

        UpdateSummary();
    }

    private void OnMontantChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (decimal.TryParse(MontantEntry.Text, out decimal montant) && montant > 0)
        {
            SummaryCard.IsVisible = true;
            TotalLabel.Text = $"{montant:N2} MAD";
        }
        else
        {
            SummaryCard.IsVisible = false;
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        // Get category from picker or custom entry
        string? categorie = CategoriePicker.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(categorie) && !string.IsNullOrWhiteSpace(CustomCategorieEntry.Text))
        {
            categorie = CustomCategorieEntry.Text.Trim();
        }

        decimal.TryParse(MontantEntry.Text, out decimal montant);

        var result = new DepenseDto
        {
            ID = _existingDepense?.ID ?? 0,
            Numero = _existingDepense?.Numero, // Will be auto-generated for new
            Date = DateOnly.FromDateTime(DatePicker.Date),
            Categorie = categorie,
            Montant = montant,
            Description = DescriptionEditor.Text
        };

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        // Validate category
        var categorie = CategoriePicker.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(categorie) && string.IsNullOrWhiteSpace(CustomCategorieEntry.Text))
        {
            ShowError("Veuillez sélectionner ou entrer une catégorie");
            return false;
        }

        // Validate amount
        if (!decimal.TryParse(MontantEntry.Text, out decimal montant) || montant <= 0)
        {
            ShowError("Montant invalide (doit être > 0)");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}