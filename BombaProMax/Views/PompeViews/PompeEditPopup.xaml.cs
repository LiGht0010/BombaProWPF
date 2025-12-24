using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PompeViews;

public partial class PompeEditPopup : Popup
{
    private readonly PompeService _pompeService;
    private readonly ReservoirService _reservoirService;
    private readonly PompeDto _pompeToEdit;
    private List<ReservoirDto> _reservoirs = new();

    public PompeEditPopup(PompeService pompeService, PompeDto pompe)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _pompeService = pompeService;
        _reservoirService = new ReservoirService();
        _pompeToEdit = pompe;

        LoadStatuses();
        LoadReservoirs();
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
            }
            else
            {
                StatutPicker.ItemsSource = new List<string> { "Actif", "Inactif", "En Maintenance", "Hors Service" };
            }

            PopulateFormBasicInfo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading statuses: {ex.Message}");
            StatutPicker.ItemsSource = new List<string> { "Actif", "Inactif", "En Maintenance", "Hors Service" };
            PopulateFormBasicInfo();
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

            PopulateFormReservoir();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reservoirs: {ex.Message}");
            ReservoirPicker.ItemsSource = new List<string> { "Erreur lors du chargement" };
            PopulateFormReservoir();
        }
    }

    private void PopulateFormBasicInfo()
    {
        if (_pompeToEdit == null) return;

        NumeroEntry.Text = _pompeToEdit.Numero ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_pompeToEdit.Statut))
        {
            var statuses = (List<string>)StatutPicker.ItemsSource;
            var statutIndex = statuses.FindIndex(s => s.Equals(_pompeToEdit.Statut, StringComparison.OrdinalIgnoreCase));
            if (statutIndex >= 0)
            {
                StatutPicker.SelectedIndex = statutIndex;
            }
        }

        CompteurElectroniqueEntry.Text = _pompeToEdit.CompteurElectroniqueActuel?.ToString() ?? "0.00";
        CompteurMecaniqueEntry.Text = _pompeToEdit.CompteurMecaniqueActuel?.ToString() ?? "0.00";
    }

    private void PopulateFormReservoir()
    {
        if (_pompeToEdit == null) return;

        if (_pompeToEdit.ReservoirAssocieID.HasValue && _reservoirs.Count > 0)
        {
            var reservoirIndex = _reservoirs.FindIndex(r => r.ID == _pompeToEdit.ReservoirAssocieID.Value);
            if (reservoirIndex >= 0)
            {
                ReservoirPicker.SelectedIndex = reservoirIndex;
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

            var numeroExists = await _pompeService.PompeNumberExistsAsync(
                NumeroEntry.Text.Trim(), _pompeToEdit.ID);
            if (numeroExists)
            {
                ShowError("Une autre pompe avec ce numéro existe déjà");
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

                if (_pompeToEdit.CompteurElectroniqueActuel.HasValue && 
                    electronValue < _pompeToEdit.CompteurElectroniqueActuel.Value)
                {
                    ShowError($"Le nouveau compteur électronique ({electronValue:N2} L) ne peut pas être inférieur à la valeur actuelle ({_pompeToEdit.CompteurElectroniqueActuel.Value:N2} L)");
                    return;
                }

                compteurElectronique = electronValue;
            }

            decimal? compteurMecanique = null;
            if (!string.IsNullOrWhiteSpace(CompteurMecaniqueEntry.Text))
            {
                if (!decimal.TryParse(CompteurMecaniqueEntry.Text, out decimal mecaniqueValue) || mecaniqueValue < 0)
                {
                    ShowError("Le compteur mécanique doit être un nombre positif ou zéro");
                    return;
                }

                if (_pompeToEdit.CompteurMecaniqueActuel.HasValue && 
                    mecaniqueValue < _pompeToEdit.CompteurMecaniqueActuel.Value)
                {
                    ShowError($"Le nouveau compteur mécanique ({mecaniqueValue:N2} L) ne peut pas être inférieur à la valeur actuelle ({_pompeToEdit.CompteurMecaniqueActuel.Value:N2} L)");
                    return;
                }

                compteurMecanique = mecaniqueValue;
            }

            _pompeToEdit.Numero = NumeroEntry.Text.Trim();
            _pompeToEdit.Statut = statut;
            _pompeToEdit.ReservoirAssocieID = reservoirId;
            _pompeToEdit.CompteurElectroniqueActuel = compteurElectronique;
            _pompeToEdit.CompteurMecaniqueActuel = compteurMecanique;

            var result = await _pompeService.UpdateAsync(_pompeToEdit);

            if (result)
            {
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la modification de la pompe");
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