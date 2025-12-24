using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DepenseViews;

public partial class DepenseCreatePopup : Popup
{
    private readonly DepenseDto? _existingDepense;
    private readonly bool _isEditMode;

    public DepenseCreatePopup(DepenseDto? existingDepense = null)
    {
        InitializeComponent();

        _existingDepense = existingDepense;
        _isEditMode = existingDepense != null;

        // Set default date
        DatePicker.Date = DateTime.Today;

        // Load categories
        LoadCategories();

        if (_isEditMode && existingDepense != null)
        {
            HeaderLabel.Text = "?? Modifier la Dépense";
            SaveButton.Text = "? Mettre à jour";
            PopulateForEdit(existingDepense);
        }

        // Handle montant changes to show summary
        MontantEntry.TextChanged += OnMontantChanged;
    }

    private void LoadCategories()
    {
        var categories = DepenseService.GetDefaultCategories();
        CategoriePicker.ItemsSource = categories;
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

        // Set category
        if (!string.IsNullOrEmpty(depense.Categorie))
        {
            var categories = CategoriePicker.ItemsSource as List<string>;
            if (categories != null && categories.Contains(depense.Categorie))
            {
                CategoriePicker.SelectedItem = depense.Categorie;
            }
            else
            {
                // Category not in list, put it in custom entry
                CustomCategorieEntry.Text = depense.Categorie;
            }
        }

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