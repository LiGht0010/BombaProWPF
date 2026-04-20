using BombaProMaxWPF.Models;
using BombaProMaxWPF.Models.Dashboard;
using System.Windows;

namespace BombaProMaxWPF.Services;

/// <summary>
/// WPF implementation of IDialogService using MessageBox and WPF Window dialogs.
/// Popup methods return null/false stubs until WPF views are created.
/// </summary>
public class DialogService : IDialogService
{
    private readonly ChauffeurService _chauffeurService;
    private readonly FournisseurService _fournisseurService;
    private readonly CiterneService _citerneService;
    private readonly CamionService _camionService;
    private readonly ProduitService _produitService;
    private readonly CategorieService _categorieService;
    private readonly ReservoirService _reservoirService;
    private readonly PompeService _pompeService;
    private readonly ClientService _clientService;
    private readonly AchatService _achatService;
    private readonly AchatAllocationService _achatAllocationService;
    private readonly UserService _userService;

    public DialogService(
        ChauffeurService chauffeurService,
        FournisseurService fournisseurService,
        CiterneService citerneService,
        CamionService camionService,
        ProduitService produitService,
        CategorieService categorieService,
        ReservoirService reservoirService,
        PompeService pompeService,
        ClientService clientService,
        AchatService achatService,
        AchatAllocationService achatAllocationService,
        UserService userService)
    {
        _chauffeurService = chauffeurService;
        _fournisseurService = fournisseurService;
        _citerneService = citerneService;
        _camionService = camionService;
        _produitService = produitService;
        _categorieService = categorieService;
        _reservoirService = reservoirService;
        _pompeService = pompeService;
        _clientService = clientService;
        _achatService = achatService;
        _achatAllocationService = achatAllocationService;
        _userService = userService;
    }

    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Oui", string cancel = "Non")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    // ????????????????????????????????????????????????????????????????
    // STUB IMPLEMENTATIONS - To be replaced with WPF Window dialogs
    // ????????????????????????????????????????????????????????????????

    public Task<ClientDto?> ShowClientCreatePopupAsync() => Task.FromResult<ClientDto?>(null);
    public Task<bool> ShowClientEditPopupAsync(ClientDto client) => Task.FromResult(false);
    public Task ShowClientDetailsPopupAsync(ClientDto client) =>
        ShowAlertAsync("Détails du client",
            $"Numéro: {client.NumeroClient}\nNom: {client.Nom}\nContact: {client.Contact ?? "N/A"}\nCIN: {client.CIN}\nSociété: {client.NomSociete}\nDate création: {client.DateCreation:dd/MM/yyyy}");

    public Task<AchatDto?> ShowAchatCreatePopupAsync() => Task.FromResult<AchatDto?>(null);
    public Task<bool> ShowAchatEditPopupAsync(AchatDto achat) => Task.FromResult(false);
    public Task ShowAchatDetailsPopupAsync(AchatDto achat) =>
        ShowAlertAsync("Détails de l'achat",
            $"Numéro: {achat.Numero}\nDate: {achat.Date:dd/MM/yyyy}\nFournisseur: {achat.FournisseurNom ?? "N/A"}\nProduit: {achat.ProduitNom ?? "N/A"}\nQuantité: {achat.Quantite} L\nCoût: {achat.Cout:N2} DH");

    public Task<ChauffeurDto?> ShowChauffeurCreatePopupAsync() => Task.FromResult<ChauffeurDto?>(null);
    public Task<bool> ShowChauffeurEditPopupAsync(ChauffeurDto chauffeur) => Task.FromResult(false);
    public Task ShowChauffeurDetailsPopupAsync(ChauffeurDto chauffeur) =>
        ShowAlertAsync("Détails du chauffeur", $"Nom: {chauffeur.Nom}\nPrénom: {chauffeur.Prenom}");

    public Task<FournisseurDto?> ShowFournisseurCreatePopupAsync() => Task.FromResult<FournisseurDto?>(null);
    public Task<bool> ShowFournisseurEditPopupAsync(FournisseurDto fournisseur) => Task.FromResult(false);
    public Task ShowFournisseurDetailsPopupAsync(FournisseurDto fournisseur) =>
        ShowAlertAsync("Détails du fournisseur", $"Nom: {fournisseur.Nom}");

    public Task<CiterneDto?> ShowCiterneCreatePopupAsync() => Task.FromResult<CiterneDto?>(null);
    public Task<bool> ShowCiterneEditPopupAsync(CiterneDto citerne) => Task.FromResult(false);
    public Task ShowCiterneDetailsPopupAsync(CiterneDto citerne) =>
        ShowAlertAsync("Détails de la citerne",
            $"ID: {citerne.ID}\nMatricule: {citerne.MatriculeCiterne ?? "N/A"}\nCapacité: {citerne.Capacite:N0} L");

    public Task<CamionDto?> ShowCamionCreatePopupAsync() => Task.FromResult<CamionDto?>(null);
    public Task<bool> ShowCamionEditPopupAsync(CamionDto camion) => Task.FromResult(false);
    public Task ShowCamionDetailsPopupAsync(CamionDto camion) =>
        ShowAlertAsync("Détails du camion", $"Matricule: {camion.Matricule}");

    public Task<ProduitDto?> ShowProduitCreatePopupAsync() => Task.FromResult<ProduitDto?>(null);
    public Task<bool> ShowProduitEditPopupAsync(ProduitDto produit) => Task.FromResult(false);
    public Task ShowProduitDetailsPopupAsync(ProduitDto produit) =>
        ShowAlertAsync("Détails du produit",
            $"Numéro: {produit.NumeroProduit}\nDescription: {produit.Description ?? "N/A"}\nPrix TTC: {produit.PrixTTC:F2} DH\nStock: {produit.Stock ?? 0}");

    public Task<ReservoirDto?> ShowReservoirCreatePopupAsync() => Task.FromResult<ReservoirDto?>(null);
    public Task<bool> ShowReservoirEditPopupAsync(ReservoirDto reservoir) => Task.FromResult(false);
    public Task ShowReservoirDetailsPopupAsync(ReservoirDto reservoir) =>
        ShowAlertAsync("Détails du réservoir",
            $"Numéro: {reservoir.Numero}\nCapacité: {reservoir.Capacite:N0} L\nNiveau: {reservoir.NiveauDeCarburant:N0} L");

    public Task<PompeDto?> ShowPompeCreatePopupAsync() => Task.FromResult<PompeDto?>(null);
    public Task<bool> ShowPompeEditPopupAsync(PompeDto pompe) => Task.FromResult(false);
    public Task ShowPompeDetailsPopupAsync(PompeDto pompe) =>
        ShowAlertAsync("Détails de la pompe",
            $"Numéro: {pompe.Numero}\nStatut: {pompe.Statut}\nRéservoir: {pompe.ReservoirNumero ?? "Non assigné"}");

    public Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupAsync(AchatDto achat) => Task.FromResult<BatchAllocationResponseDto?>(null);
    public Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupForNewAchatAsync(AchatDto achat) => Task.FromResult<BatchAllocationResponseDto?>(null);
    public Task<BatchAllocationResponseDto?> ClearAndShowAllocationPopupAsync(AchatDto achat) => Task.FromResult<BatchAllocationResponseDto?>(null);

    public Task ShowClientCreditManagementAsync(ClientDto client)
    {
        // TODO: Navigate to credit management view in WPF
        return ShowAlertAsync("Navigation", $"Gestion de crédit pour le client: {client.Nom} (à implémenter)");
    }

    public Task ShowAchatPerProductPopupAsync(ProductCardModel product, List<AchatAnalyticsRowDto> achats) =>
        ShowAlertAsync("Achats par produit", $"{product.ProduitNom}: {achats.Count} achat(s)");

    public Task ShowVentePerProductPopupAsync(ProductCardModel product, List<VenteAnalyticsRowDto> ventes) =>
        ShowAlertAsync("Ventes par produit", $"{product.ProduitNom}: {ventes.Count} vente(s)");

    public Task ShowVenteCarburantPerProductPopupAsync(ProductCardModel product, List<VenteCarburantAnalyticsRowDto> ventes) =>
        ShowAlertAsync("Ventes carburant par produit", $"{product.ProduitNom}: {ventes.Count} vente(s)");

    public Task<UserDto?> ShowUserCreatePopupAsync() => Task.FromResult<UserDto?>(null);
    public Task<bool> ShowUserEditPopupAsync(UserDto user) => Task.FromResult(false);
    public Task ShowUserDetailsPopupAsync(UserDto user) =>
        ShowAlertAsync("Détails utilisateur", $"Nom: {user.Name}\nEmail: {user.Email}");

    public Task<StationInfoDto?> ShowStationInfoEditPopupAsync(StationInfoDto? existingStationInfo) => Task.FromResult<StationInfoDto?>(null);
    public Task<bool> ShowPasswordConfirmationPopupAsync() => Task.FromResult(false);
    public Task<StockWithdrawalResponseDto?> ShowStockWithdrawalCreatePopupAsync() => Task.FromResult<StockWithdrawalResponseDto?>(null);
}
