using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombaProMax.Views.ReservoirViews;

public partial class ReservoirCreatePopup : Popup
{
    private readonly ReservoirService _reservoirService;
    private readonly ProduitService _produitService;
    private List<ProduitDto> _produits = new();

    public ReservoirCreatePopup(ReservoirService reservoirService, ProduitService produitService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _reservoirService = reservoirService;
        _produitService = produitService;

        LoadProduits();
        GenerateReservoirNumber();
    }

    private async void LoadProduits()
    {
        try
        {
            _produits = await _produitService.GetAllProduitsAsync();

            if (_produits.Count > 0)
            {
                ProduitPicker.ItemsSource = _produits.Select(p => 
                    string.IsNullOrWhiteSpace(p.Description) ? p.NumeroProduit : p.Description).ToList();
            }
            else
            {
                ProduitPicker.ItemsSource = new List<string> { "Aucun produit disponible" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            ProduitPicker.ItemsSource = new List<string> { "Erreur lors du chargement" };
        }
    }

    private void GenerateReservoirNumber()
    {
        var year = DateTime.Now.Year;
        var random = new Random().Next(100, 999);
        NumeroEntry.Text = $"RES-{year}-{random}";
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        try
        {
            ErrorLabel.IsVisible = false;

            if (string.IsNullOrWhiteSpace(NumeroEntry.Text))
            {
                ShowError("Le numéro est obligatoire");
                return;
            }

            if (ProduitPicker.SelectedIndex < 0)
            {
                ShowError("Le type de carburant est obligatoire");
                return;
            }

            if (string.IsNullOrWhiteSpace(CapaciteEntry.Text))
            {
                ShowError("La capacité est obligatoire");
                return;
            }

            if (string.IsNullOrWhiteSpace(NiveauEntry.Text))
            {
                ShowError("Le niveau initial est obligatoire");
                return;
            }

            var numeroExists = await _reservoirService.ReservoirNumberExistsAsync(NumeroEntry.Text.Trim());
            if (numeroExists)
            {
                ShowError("Un réservoir avec ce numéro existe déjà");
                return;
            }

            if (!decimal.TryParse(CapaciteEntry.Text, out decimal capacite) || capacite <= 0)
            {
                ShowError("La capacité doit être un nombre positif");
                return;
            }

            if (!decimal.TryParse(NiveauEntry.Text, out decimal niveau) || niveau < 0)
            {
                ShowError("Le niveau doit être un nombre positif ou zéro");
                return;
            }

            if (niveau > capacite)
            {
                ShowError("Le niveau initial ne peut pas dépasser la capacité");
                return;
            }

            int? produitId = null;
            string? produitNom = null;
            if (ProduitPicker.SelectedIndex >= 0 && _produits.Count > 0)
            {
                var selectedProduit = _produits[ProduitPicker.SelectedIndex];
                produitId = selectedProduit.ID;
                produitNom = selectedProduit.Description ?? selectedProduit.NumeroProduit;
            }

            var newReservoir = new ReservoirDto
            {
                Numero = NumeroEntry.Text.Trim(),
                ProduitID = produitId,
                ProduitNom = produitNom,
                Capacite = capacite,
                NiveauDeCarburant = niveau
            };

            var result = await _reservoirService.CreateReservoirAsync(newReservoir);

            if (result != null)
            {
                await CloseAsync(result); // Return the created DTO
            }
            else
            {
                ShowError("Erreur lors de la création du réservoir");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Une erreur s'est produite: {ex.Message}");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}