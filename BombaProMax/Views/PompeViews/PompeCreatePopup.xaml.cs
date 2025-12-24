using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombaProMax.Views.PompeViews;

public partial class PompeCreatePopup : Popup
{
    private readonly PompeService _pompeService;
    private readonly ReservoirService _reservoirService;
    private List<ReservoirDto> _reservoirs = new();

    public PompeCreatePopup(PompeService pompeService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _pompeService = pompeService;
        _reservoirService = new ReservoirService();

        LoadStatuses();
        LoadReservoirs();
        GeneratePompeNumber();
    }

    private async void LoadStatuses()
    {
        try
        {
            var allPompes = await _pompeService.GetAllAsync();
            
            var uniqueStatuses = allPompes
                .Where(p => !string.IsNullOrWhiteSpace(p.Statut))
                .Select(p => p.Statut)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            if (uniqueStatuses.Count > 0)
            {
                StatutPicker.ItemsSource = uniqueStatuses;
                var actifIndex = uniqueStatuses.FindIndex(s => s!.Equals("Actif", StringComparison.OrdinalIgnoreCase));
                if (actifIndex >= 0)
                {
                    StatutPicker.SelectedIndex = actifIndex;
                }
            }
            else
            {
                StatutPicker.ItemsSource = new List<string> { "Actif", "Inactif", "En Maintenance", "Hors Service" };
                StatutPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading statuses: {ex.Message}");
            StatutPicker.ItemsSource = new List<string> { "Actif", "Inactif", "En Maintenance", "Hors Service" };
            StatutPicker.SelectedIndex = 0;
        }
    }

    private async void LoadReservoirs()
    {
        try
        {
            _reservoirs = await _reservoirService.GetAllReservoirsAsync();

            if (_reservoirs.Count > 0)
            {
                ReservoirPicker.ItemsSource = _reservoirs.Select(r => 
                    $"{r.Numero} - {r.ProduitNom ?? "Non spécifié"}").ToList();
            }
            else
            {
                ReservoirPicker.ItemsSource = new List<string> { "Aucun réservoir disponible" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reservoirs: {ex.Message}");
            ReservoirPicker.ItemsSource = new List<string> { "Erreur lors du chargement" };
        }
    }

    private void GeneratePompeNumber()
    {
        var year = DateTime.Now.Year;
        var random = new Random().Next(100, 999);
        NumeroEntry.Text = $"P-{year}-{random}";
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

            if (StatutPicker.SelectedIndex < 0)
            {
                ShowError("Le statut est obligatoire");
                return;
            }

            if (ReservoirPicker.SelectedIndex < 0)
            {
                ShowError("Le réservoir associé est obligatoire");
                return;
            }

            var numeroExists = await _pompeService.PompeNumberExistsAsync(NumeroEntry.Text.Trim());
            if (numeroExists)
            {
                ShowError("Une pompe avec ce numéro existe déjà");
                return;
            }

            string statut = ((List<string>)StatutPicker.ItemsSource)[StatutPicker.SelectedIndex];

            int? reservoirId = null;
            if (ReservoirPicker.SelectedIndex >= 0 && _reservoirs.Count > 0)
            {
                reservoirId = _reservoirs[ReservoirPicker.SelectedIndex].ID;
            }

            decimal? compteurElectronique = null;
            if (!string.IsNullOrWhiteSpace(CompteurElectroniqueEntry.Text))
            {
                if (!decimal.TryParse(CompteurElectroniqueEntry.Text, out decimal electronValue) || electronValue < 0)
                {
                    ShowError("Le compteur électronique doit être un nombre positif ou zéro");
                    return;
                }
                compteurElectronique = electronValue;
            }
            else
            {
                compteurElectronique = 0m;
            }

            decimal? compteurMecanique = null;
            if (!string.IsNullOrWhiteSpace(CompteurMecaniqueEntry.Text))
            {
                if (!decimal.TryParse(CompteurMecaniqueEntry.Text, out decimal mecaniqueValue) || mecaniqueValue < 0)
                {
                    ShowError("Le compteur mécanique doit être un nombre positif ou zéro");
                    return;
                }
                compteurMecanique = mecaniqueValue;
            }
            else
            {
                compteurMecanique = 0m;
            }

            var newPompe = new PompeDto
            {
                Numero = NumeroEntry.Text.Trim(),
                Statut = statut,
                ReservoirAssocieID = reservoirId,
                CompteurElectroniqueActuel = compteurElectronique,
                CompteurMecaniqueActuel = compteurMecanique
            };

            var result = await _pompeService.CreateAsync(newPompe);

            if (result != null)
            {
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la création de la pompe");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
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