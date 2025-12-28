using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ServiceViews;

public partial class ServiceCategorieCreatePopup : Popup
{
    private readonly ServiceCategorieDto? _existingCategorie;
    private readonly bool _isEditMode;

    public ServiceCategorieCreatePopup(ServiceCategorieDto? existingCategorie = null)
    {
        InitializeComponent();

        _existingCategorie = existingCategorie;
        _isEditMode = existingCategorie != null;

        if (_isEditMode && existingCategorie != null)
        {
            HeaderLabel.Text = "Modifier la Categorie";
            SaveButton.Text = "Mettre a jour";
            PopulateForEdit(existingCategorie);
        }
    }

    private void PopulateForEdit(ServiceCategorieDto categorie)
    {
        NomEntry.Text = categorie.Nom;
        DescriptionEditor.Text = categorie.Description;
        IsActiveSwitch.IsToggled = categorie.IsActive;
    }

    private void OnQuickCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            NomEntry.Text = button.Text.Trim();
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

        var result = new ServiceCategorieDto
        {
            ID = _existingCategorie?.ID ?? 0,
            Nom = NomEntry.Text?.Trim(),
            Description = DescriptionEditor.Text?.Trim(),
            IsActive = IsActiveSwitch.IsToggled
        };

        Close(result);
    }

    private bool ValidateForm()
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(NomEntry.Text))
        {
            ShowError("Le nom de la categorie est obligatoire");
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