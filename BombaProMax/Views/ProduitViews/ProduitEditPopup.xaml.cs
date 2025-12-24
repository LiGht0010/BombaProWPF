using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.ProduitViews;

public partial class ProduitEditPopup : Popup
{
    private readonly ProduitService _produitService;
    private readonly CategorieService _categorieService;
    private List<CategorieDto> _categories = new();
    private readonly ProduitDto _produit;

    public ProduitEditPopup(ProduitService produitService, CategorieService categorieService, ProduitDto produit)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _produitService = produitService;
        _categorieService = categorieService;
        _produit = produit;

        LoadCategories();
        LoadProduitData();
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
                
                // Select the current product's category
                if (_produit.CategorieID.HasValue)
                {
                    var currentCategory = _categories.FirstOrDefault(c => c.ID == _produit.CategorieID.Value);
                    if (currentCategory != null)
                    {
                        var index = _categories.IndexOf(currentCategory);
                        if (index >= 0)
                        {
                            CategoriePicker.SelectedIndex = index;
                        }
                    }
                }
            }
            else
            {
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

    private void LoadProduitData()
    {
        if (_produit != null)
        {
            NumeroProduitEntry.Text = _produit.NumeroProduit;
            DescriptionEntry.Text = _produit.Description;
            PrixAchatEntry.Text = _produit.PrixAchat?.ToString("F2") ?? "0.00";
            PrixHTEntry.Text = _produit.PrixHT?.ToString("F2") ?? "0.00";
            TVAEntry.Text = _produit.TVA?.ToString("F0") ?? "20";
            StockEntry.Text = _produit.Stock?.ToString() ?? "0";
            StockMinimumEntry.Text = _produit.StockMinimum?.ToString() ?? "0";
            DelaiLivraisonEntry.Text = _produit.DelaiDeLivraison?.ToString() ?? "";
            
            // Calculate and display current prices
            CalculatePrices();
        }
    }

    private void CalculatePrices()
    {
        try
        {
            decimal prixAchat = string.IsNullOrWhiteSpace(PrixAchatEntry.Text) ? 0 : decimal.Parse(PrixAchatEntry.Text);
            decimal prixHT = string.IsNullOrWhiteSpace(PrixHTEntry.Text) ? 0 : decimal.Parse(PrixHTEntry.Text);
            decimal tva = string.IsNullOrWhiteSpace(TVAEntry.Text) ? 20 : decimal.Parse(TVAEntry.Text);

            // Calculate PrixTTC from PrixHT and TVA
            decimal prixTTC = prixHT * (1 + tva / 100);
            PrixTTCLabel.Text = $"{prixTTC:F2} DH";

            // Calculate margins
            decimal marge = prixHT - prixAchat;
            MargeBeneficiaireLabel.Text = $"{marge:F2} DH";

            if (prixAchat > 0)
            {
                decimal margePourcentage = (marge / prixAchat) * 100;
                MargePourcentageLabel.Text = $"{margePourcentage:F2}%";
                
                // Color code the margin
                if (margePourcentage < 10)
                    MargePourcentageLabel.TextColor = Color.FromArgb("#F44336"); // Red
                else if (margePourcentage < 20)
                    MargePourcentageLabel.TextColor = Color.FromArgb("#FF9800"); // Orange
                else
                    MargePourcentageLabel.TextColor = Color.FromArgb("#4CAF50"); // Green
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
        Close(false);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
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
            // Update the product DTO
            _produit.NumeroProduit = NumeroProduitEntry.Text.Trim();
            _produit.Description = DescriptionEntry.Text.Trim();
            _produit.PrixAchat = string.IsNullOrWhiteSpace(PrixAchatEntry.Text) ? null : decimal.Parse(PrixAchatEntry.Text);
            _produit.PrixHT = string.IsNullOrWhiteSpace(PrixHTEntry.Text) ? null : decimal.Parse(PrixHTEntry.Text);
            _produit.TVA = string.IsNullOrWhiteSpace(TVAEntry.Text) ? 20 : decimal.Parse(TVAEntry.Text);
            _produit.Stock = string.IsNullOrWhiteSpace(StockEntry.Text) ? 0 : int.Parse(StockEntry.Text);
            _produit.StockMinimum = string.IsNullOrWhiteSpace(StockMinimumEntry.Text) ? 0 : int.Parse(StockMinimumEntry.Text);
            _produit.DelaiDeLivraison = string.IsNullOrWhiteSpace(DelaiLivraisonEntry.Text) ? null : int.Parse(DelaiLivraisonEntry.Text);

            // Set CategorieID based on selected category
            if (CategoriePicker.SelectedIndex >= 0 && _categories.Count > 0)
            {
                var selectedCategoryName = CategoriePicker.SelectedItem?.ToString();
                var selectedCategory = _categories.FirstOrDefault(c => c.Nom == selectedCategoryName);
                if (selectedCategory != null)
                {
                    _produit.CategorieID = selectedCategory.ID;
                    _produit.CategorieNom = selectedCategory.Nom;
                }
            }

            var result = await _produitService.UpdateProduitAsync(_produit);

            if (result)
            {
                await CloseAsync(true);
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la mise à jour du produit";
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