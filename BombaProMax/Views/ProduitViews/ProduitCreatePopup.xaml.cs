using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.ProduitViews;

public partial class ProduitCreatePopup : Popup
{
    private readonly ProduitService _produitService;
    private readonly CategorieService _categorieService;
    private List<CategorieDto> _categories = new();

    public ProduitCreatePopup(CategorieService categorieService, ProduitService produitService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        _produitService = produitService;
        _categorieService = categorieService;

        LoadCategories();
    }

    private async void LoadCategories()
    {
        try
        {
            CategoriePicker.Items.Clear();
            
            _categories = await _categorieService.GetAllCategoriesAsync();
            
            if (_categories.Count > 0)
            {
                foreach (var category in _categories)
                {
                    CategoriePicker.Items.Add(category.Nom);
                }
                
                if (CategoriePicker.Items.Count > 0)
                {
                    CategoriePicker.SelectedIndex = 0;
                }
            }
            else
            {
                // Fallback defaults
                CategoriePicker.Items.Add("Carburant");
                CategoriePicker.Items.Add("Lubrifiant");
                CategoriePicker.Items.Add("Articles");
                CategoriePicker.Items.Add("Accessoires");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading categories: {ex.Message}");
            
            CategoriePicker.Items.Clear();
            CategoriePicker.Items.Add("Carburant");
            CategoriePicker.Items.Add("Lubrifiant");
            CategoriePicker.Items.Add("Articles");
            CategoriePicker.Items.Add("Accessoires");
        }
    }

    private void CalculatePrices()
    {
        try
        {
            decimal prixAchat = string.IsNullOrWhiteSpace(PrixAchatEntry.Text) ? 0 : decimal.Parse(PrixAchatEntry.Text);
            decimal prixHT = string.IsNullOrWhiteSpace(PrixHTEntry.Text) ? 0 : decimal.Parse(PrixHTEntry.Text);
            decimal tva = string.IsNullOrWhiteSpace(TVAEntry.Text) ? 20 : decimal.Parse(TVAEntry.Text);

            decimal prixTTC = prixHT * (1 + tva / 100);
            PrixTTCLabel.Text = $"{prixTTC:F2} DH";

            decimal marge = prixHT - prixAchat;
            MargeBeneficiaireLabel.Text = $"{marge:F2} DH";

            if (prixAchat > 0)
            {
                decimal margePourcentage = (marge / prixAchat) * 100;
                MargePourcentageLabel.Text = $"{margePourcentage:F2}%";
                
                if (margePourcentage < 10)
                    MargePourcentageLabel.TextColor = Color.FromArgb("#F44336");
                else if (margePourcentage < 20)
                    MargePourcentageLabel.TextColor = Color.FromArgb("#FF9800");
                else
                    MargePourcentageLabel.TextColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                MargePourcentageLabel.Text = "0.00%";
            }
        }
        catch
        {
            PrixTTCLabel.Text = "0.00 DH";
            MargeBeneficiaireLabel.Text = "0.00 DH";
            MargePourcentageLabel.Text = "0.00%";
        }
    }

    private void OnPrixAchatChanged(object sender, TextChangedEventArgs e) => CalculatePrices();
    private void OnPrixHTChanged(object sender, TextChangedEventArgs e) => CalculatePrices();
    private void OnTVAChanged(object sender, TextChangedEventArgs e) => CalculatePrices();

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NumeroProduitEntry.Text))
        {
            ErrorLabel.Text = "Le numéro de produit est requis";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            ErrorLabel.Text = "La description est requise";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            var produit = new ProduitDto
            {
                NumeroProduit = NumeroProduitEntry.Text.Trim(),
                Description = DescriptionEntry.Text.Trim(),
                PrixAchat = string.IsNullOrWhiteSpace(PrixAchatEntry.Text) ? null : decimal.Parse(PrixAchatEntry.Text),
                PrixHT = string.IsNullOrWhiteSpace(PrixHTEntry.Text) ? null : decimal.Parse(PrixHTEntry.Text),
                TVA = string.IsNullOrWhiteSpace(TVAEntry.Text) ? 20 : decimal.Parse(TVAEntry.Text),
                Stock = string.IsNullOrWhiteSpace(StockEntry.Text) ? 0 : int.Parse(StockEntry.Text),
                StockMinimum = string.IsNullOrWhiteSpace(StockMinimumEntry.Text) ? 0 : int.Parse(StockMinimumEntry.Text),
                DelaiDeLivraison = string.IsNullOrWhiteSpace(DelaiLivraisonEntry.Text) ? null : int.Parse(DelaiLivraisonEntry.Text)
            };

            // Set CategorieID based on selected category
            if (CategoriePicker.SelectedIndex >= 0 && _categories.Count > 0)
            {
                var selectedCategoryName = CategoriePicker.SelectedItem?.ToString();
                var selectedCategory = _categories.FirstOrDefault(c => c.Nom == selectedCategoryName);
                if (selectedCategory != null)
                {
                    produit.CategorieID = selectedCategory.ID;
                    produit.CategorieNom = selectedCategory.Nom;
                }
            }

            var result = await _produitService.CreateProduitAsync(produit);

            if (result != null)
            {
                await CloseAsync(result); // Return the created DTO
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la création du produit";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (FormatException)
        {
            ErrorLabel.Text = "Veuillez entrer des valeurs numériques valides";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }
}