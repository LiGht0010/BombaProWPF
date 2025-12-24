using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ClientViews;

public partial class ClientCreatePopup : Popup
{
    private readonly ClientService _clientService;

    public ClientCreatePopup(ClientService clientService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        _clientService = clientService;

        // Generate automatic client number
        GenerateClientNumber();
    }

    private void GenerateClientNumber()
    {
        var year = DateTime.Now.Year;
        var random = new Random().Next(1000, 9999);
        NumeroClientEntry.Text = $"CLI-{year}-{random}";
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        try
        {
            ErrorLabel.IsVisible = false;

            // Validate input
            if (string.IsNullOrWhiteSpace(NumeroClientEntry.Text))
            {
                ShowError("Le numéro client est requis");
                return;
            }

            if (string.IsNullOrWhiteSpace(NomEntry.Text))
            {
                ShowError("Le nom est requis");
                return;
            }

            if (string.IsNullOrWhiteSpace(CINEntry.Text))
            {
                ShowError("Le CIN est requis");
                return;
            }

            if (string.IsNullOrWhiteSpace(NomSocieteEntry.Text))
            {
                ShowError("Le nom de la société est requis");
                return;
            }

            // Check if client number already exists
            var numeroExists = await _clientService.ClientNumberExistsAsync(NumeroClientEntry.Text.Trim());
            if (numeroExists)
            {
                ShowError("Un client avec ce numéro existe déjà");
                return;
            }

            // Check if CIN already exists
            var cinExists = await _clientService.ClientCINExistsAsync(CINEntry.Text.Trim());
            if (cinExists)
            {
                ShowError("Un client avec ce CIN existe déjà");
                return;
            }

            // Create new client DTO
            var newClient = new ClientDto
            {
                NumeroClient = NumeroClientEntry.Text.Trim(),
                Nom = NomEntry.Text.Trim(),
                Contact = ContactEntry.Text?.Trim(),
                CIN = CINEntry.Text.Trim(),
                NomSociete = NomSocieteEntry.Text.Trim()
            };

            // Save to database via service
            var createdClient = await _clientService.CreateClientAsync(newClient);

            if (createdClient != null)
            {
                await CloseAsync(createdClient);
            }
            else
            {
                ShowError("Erreur lors de la création du client");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
