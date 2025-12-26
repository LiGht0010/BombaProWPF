using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DepenseViews;

public partial class DepenseCategorieCreatePopup : Popup
{
    private readonly DepenseCategorieDto? _existingCategorie;
    private readonly bool _isEditMode;

    public DepenseCategorieCreatePopup(DepenseCategorieDto? existingCategorie = null)
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

    private void PopulateForEdit(DepenseCategorieDto categorie)
    {
        NomEntry.Text = categorie.Nom;
        DescriptionEditor.Text = categorie.Description;
        IsActiveSwitch.IsToggled = categorie.IsActive;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateForm())
            return;

        var result = new DepenseCategorieDto
        {
            ID = _existingCategorie?.ID ?? 0,
            Nom = NomEntry.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(DescriptionEditor.Text) ? null : DescriptionEditor.Text.Trim(),
            IsActive = IsActiveSwitch.IsToggled,
            CreePar = _existingCategorie?.CreePar,
            DateCreation = _existingCategorie?.DateCreation
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

        if (NomEntry.Text.Trim().Length < 2)
        {
            ShowError("Le nom doit contenir au moins 2 caracteres");
            return false;
        }

        if (NomEntry.Text.Trim().Length > 100)
        {
            ShowError("Le nom ne peut pas depasser 100 caracteres");
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
