using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombaProMax.Views.ReservoirViews;

public partial class ReservoirEditPopup : Popup
{
    private readonly ReservoirService _reservoirService;
    private readonly ProduitService _produitService;
    private readonly ReservoirDto _reservoirToEdit;
    private List<ProduitDto> _produits = new();

    public ReservoirEditPopup(ReservoirService reservoirService, ProduitService produitService, ReservoirDto reservoir)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _reservoirService = reservoirService;
        _produitService = produitService;
        _reservoirToEdit = reservoir;

        LoadProduits();
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

            PopulateForm();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            ProduitPicker.ItemsSource = new List<string> { "Erreur lors du chargement" };
            PopulateForm();
        }
    }

    private void PopulateForm()
    {
        if (_reservoirToEdit == null) return;

        NumeroEntry.Text = _reservoirToEdit.Numero ?? string.Empty;
        CapaciteEntry.Text = _reservoirToEdit.Capacite.ToString();
        NiveauEntry.Text = _reservoirToEdit.NiveauDeCarburant.ToString();

        if (_reservoirToEdit.ProduitID.HasValue && _produits.Count > 0)
        {
            var produitIndex = _produits.FindIndex(p => p.ID == _reservoirToEdit.ProduitID.Value);
            if (produitIndex >= 0)
            {
                ProduitPicker.SelectedIndex = produitIndex;
            }
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
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
                ShowError("Le niveau est obligatoire");
                return;
            }

            var numeroExists = await _reservoirService.ReservoirNumberExistsAsync(
                NumeroEntry.Text.Trim(), _reservoirToEdit.ID);
            if (numeroExists)
            {
                ShowError("Un autre réservoir avec ce numéro existe déjà");
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
                ShowError("Le niveau ne peut pas dépasser la capacité");
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

            // Update the DTO
            _reservoirToEdit.Numero = NumeroEntry.Text.Trim();
            _reservoirToEdit.ProduitID = produitId;
            _reservoirToEdit.ProduitNom = produitNom;
            _reservoirToEdit.Capacite = capacite;
            _reservoirToEdit.NiveauDeCarburant = niveau;

            var result = await _reservoirService.UpdateReservoirAsync(_reservoirToEdit);

            if (result)
            {
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la modification du réservoir");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Une erreur s'est produite: {ex.Message}");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}