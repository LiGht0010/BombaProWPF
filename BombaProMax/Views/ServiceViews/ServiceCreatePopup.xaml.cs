using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ServiceViews;

public partial class ServiceCreatePopup : Popup
{
    private readonly ServiceDto? _existingService;
    private readonly bool _isEditMode;
    private readonly List<ServiceCategorieDto> _categories;

    public ServiceCreatePopup(List<ServiceCategorieDto> categories, ServiceDto? existingService = null)
    {
        InitializeComponent();

        _categories = categories;
        _existingService = existingService;
        _isEditMode = existingService != null;

        // Load categories into picker
        LoadCategories();

        if (_isEditMode && existingService != null)
        {
            HeaderLabel.Text = "Modifier le Service";
            SaveButton.Text = "Mettre a jour";
            PopulateForEdit(existingService);
        }
        else
        {
            // Auto-generate numero for new service
            NumeroEntry.Text = GenerateServiceNumero();
        }
    }

    private static string GenerateServiceNumero()
    {
        return $"SRV{DateTime.Now:yyyyMMddHHmmss}";
    }

    private void LoadCategories()
    {
        var categoryNames = _categories.Select(c => c.Nom).ToList();
        CategoriePicker.ItemsSource = categoryNames;
    }

    private void PopulateForEdit(ServiceDto service)
    {
        NumeroEntry.Text = service.Numero;
        DescriptionEntry.Text = service.Description;
        PrixEntry.Text = service.Prix?.ToString("F2");

        // Set category
        if (!string.IsNullOrEmpty(service.ServiceCategorieNom))
        {
            var index = _categories.FindIndex(c => c.Nom == service.ServiceCategorieNom);
            if (index >= 0)
            {
                CategoriePicker.SelectedIndex = index;
            }
        }
    }

    private void OnQuickPriceClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var priceText = button.Text.Replace(" DH", "").Trim();
            if (decimal.TryParse(priceText, out var price))
            {
                PrixEntry.Text = price.ToString("F2");
            }
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        // Get category
        ServiceCategorieDto? selectedCategory = null;
        if (CategoriePicker.SelectedIndex >= 0 && CategoriePicker.SelectedIndex < _categories.Count)
        {
            selectedCategory = _categories[CategoriePicker.SelectedIndex];
        }

        decimal.TryParse(PrixEntry.Text, out decimal prix);

        var result = new ServiceDto
        {
            ID = _existingService?.ID ?? 0,
            Numero = NumeroEntry.Text?.Trim(),
            Description = DescriptionEntry.Text?.Trim(),
            Prix = prix,
            ServiceCategorieID = selectedCategory?.ID,
            ServiceCategorieNom = selectedCategory?.Nom
        };

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        // Validate description
        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            ShowError("La description est obligatoire");
            return false;
        }

        // Validate price
        if (!decimal.TryParse(PrixEntry.Text, out decimal prix) || prix < 0)
        {
            ShowError("Prix invalide (doit etre >= 0)");
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