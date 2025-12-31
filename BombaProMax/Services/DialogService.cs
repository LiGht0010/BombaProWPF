using BombaProMax.Models;
using BombaProMax.Models.Dashboard;
using BombaProMax.Views.ClientViews;
using BombaProMax.Views.AchatViews;
using BombaProMax.Views.ChauffeurViews;
using BombaProMax.Views.FournisseurViews;
using BombaProMax.Views.CiterneViews;
using BombaProMax.Views.CamionViews;
using BombaProMax.Views.ProduitViews;
using BombaProMax.Views.ReservoirViews;
using BombaProMax.Views.PompeViews;
using BombaProMax.Views.DashboardViews;
using BombaProMax.Views.User;
using BombaProMax.Views.SettingsViews;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Services;

/// <summary>
/// Implementation of IDialogService that uses MAUI alerts and CommunityToolkit popups.
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

    private Page CurrentPage => Application.Current?.MainPage 
        ?? throw new InvalidOperationException("No main page available");

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

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        await CurrentPage.DisplayAlert(title, message, cancel);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Oui", string cancel = "Non")
    {
        return await CurrentPage.DisplayAlert(title, message, accept, cancel);
    }

    public async Task<ClientDto?> ShowClientCreatePopupAsync()
    {
        var popup = new ClientCreatePopup(_clientService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as ClientDto;
    }

    public async Task<bool> ShowClientEditPopupAsync(ClientDto client)
    {
        var popup = new ClientEditPopup(_clientService, client);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowClientDetailsPopupAsync(ClientDto client)
    {
        await ShowAlertAsync("Détails du client",
            $"Numéro: {client.NumeroClient}\n" +
            $"Nom: {client.Nom}\n" +
            $"Contact: {client.Contact ?? "N/A"}\n" +
            $"CIN: {client.CIN}\n" +
            $"Société: {client.NomSociete}\n" +
            $"Date création: {client.DateCreation:dd/MM/yyyy}");
    }

    public async Task<AchatDto?> ShowAchatCreatePopupAsync()
    {
        var popup = new AchatCreatePopup(
            _achatService,
            _fournisseurService,
            _produitService,
            _camionService,
            _chauffeurService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as AchatDto;
    }

    public async Task<bool> ShowAchatEditPopupAsync(AchatDto achat)
    {
        var popup = new AchatEditPopup(
            _achatService,
            _fournisseurService,
            _produitService,
            _camionService,
            _chauffeurService,
            achat);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowAchatDetailsPopupAsync(AchatDto achat)
    {
        var popup = new AchatDetailsPopup(achat);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task<ChauffeurDto?> ShowChauffeurCreatePopupAsync()
    {
        var popup = new ChauffeurCreatePopup(_chauffeurService, _fournisseurService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as ChauffeurDto;
    }

    public async Task<bool> ShowChauffeurEditPopupAsync(ChauffeurDto chauffeur)
    {
        var popup = new ChauffeurEditPopup(chauffeur);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowChauffeurDetailsPopupAsync(ChauffeurDto chauffeur)
    {
        var popup = new ChauffeurDetailsPopup(chauffeur);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task<FournisseurDto?> ShowFournisseurCreatePopupAsync()
    {
        var popup = new FournisseurCreatePopup(_fournisseurService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as FournisseurDto;
    }

    public async Task<bool> ShowFournisseurEditPopupAsync(FournisseurDto fournisseur)
    {
        var popup = new FournisseurEditPopup(_fournisseurService, fournisseur);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowFournisseurDetailsPopupAsync(FournisseurDto fournisseur)
    {
        var popup = new FournisseurDetailsPopup(fournisseur);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task<CiterneDto?> ShowCiterneCreatePopupAsync()
    {
        var popup = new CiterneCreatePopup(_citerneService, _fournisseurService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as CiterneDto;
    }

    public async Task<bool> ShowCiterneEditPopupAsync(CiterneDto citerne)
    {
        var popup = new CiterneEditPopup(_citerneService, _fournisseurService, citerne);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowCiterneDetailsPopupAsync(CiterneDto citerne)
    {
        await ShowAlertAsync("Détails de la citerne",
            $"ID: {citerne.ID}\n" +
            $"Matricule: {citerne.MatriculeCiterne ?? "N/A"}\n" +
            $"Capacité: {citerne.Capacite:N0} L\n" +
            $"Partitions: {citerne.PartitionsNumber ?? 0}\n" +
            $"Fournisseur: {citerne.FournisseurNom ?? "N/A"}\n" +
            $"Date création: {citerne.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A"}");
    }

    public async Task<CamionDto?> ShowCamionCreatePopupAsync()
    {
        var popup = new CamionCreatePopup(_camionService, _fournisseurService, _citerneService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as CamionDto;
    }

    public async Task<bool> ShowCamionEditPopupAsync(CamionDto camion)
    {
        var popup = new CamionEditPopup(_camionService, _fournisseurService, _citerneService, camion);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowCamionDetailsPopupAsync(CamionDto camion)
    {
        var popup = new CamionDetailsPopup(camion);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task<ProduitDto?> ShowProduitCreatePopupAsync()
    {
        var popup = new ProduitCreatePopup(_categorieService, _produitService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as ProduitDto;
    }

    public async Task<bool> ShowProduitEditPopupAsync(ProduitDto produit)
    {
        var popup = new ProduitEditPopup(_produitService, _categorieService, produit);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowProduitDetailsPopupAsync(ProduitDto produit)
    {
        await ShowAlertAsync("Détails du produit",
            $"Numéro: {produit.NumeroProduit}\n" +
            $"Description: {produit.Description ?? "N/A"}\n" +
            $"Catégorie: {produit.CategorieNom ?? "Non categorizado"}\n" +
            $"Prix d'achat: {produit.PrixAchat:F2} DH\n" +
            $"Prix HT: {produit.PrixHT:F2} DH\n" +
            $"TVA: {produit.TVA}%\n" +
            $"Prix TTC: {produit.PrixTTC:F2} DH\n" +
            $"Stock: {produit.Stock ?? 0}\n" +
            $"Stock Min: {produit.StockMinimum ?? 0}\n" +
            $"Date création: {produit.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A"}");
    }

    public async Task<ReservoirDto?> ShowReservoirCreatePopupAsync()
    {
        var popup = new ReservoirCreatePopup(_reservoirService, _produitService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as ReservoirDto;
    }

    public async Task<bool> ShowReservoirEditPopupAsync(ReservoirDto reservoir)
    {
        var popup = new ReservoirEditPopup(_reservoirService, _produitService, reservoir);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowReservoirDetailsPopupAsync(ReservoirDto reservoir)
    {
        var fillPercent = reservoir.Capacite > 0 
            ? Math.Round((reservoir.NiveauDeCarburant / reservoir.Capacite) * 100, 1) 
            : 0;

        await ShowAlertAsync("Détails du réservoir",
            $"ID: {reservoir.ID}\n" +
            $"Numéro: {reservoir.Numero}\n" +
            $"Type carburant: {reservoir.ProduitNom ?? "Non assigné"}\n" +
            $"Capacité: {reservoir.Capacite:N0} L\n" +
            $"Niveau actuel: {reservoir.NiveauDeCarburant:N0} L\n" +
            $"Taux de remplissage: {fillPercent}%\n" +
            $"Date création: {reservoir.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A"}");
    }

    public async Task<PompeDto?> ShowPompeCreatePopupAsync()
    {
        var popup = new PompeCreatePopup(_pompeService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as PompeDto;
    }

    public async Task<bool> ShowPompeEditPopupAsync(PompeDto pompe)
    {
        var popup = new PompeEditPopup(_pompeService, pompe);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowPompeDetailsPopupAsync(PompeDto pompe)
    {
        var discrepancy = pompe.CompteurElectroniqueActuel.HasValue && pompe.CompteurMecaniqueActuel.HasValue
            ? Math.Abs(pompe.CompteurElectroniqueActuel.Value - pompe.CompteurMecaniqueActuel.Value)
            : 0;

        await ShowAlertAsync("Détails de la pompe",
            $"ID: {pompe.ID}\n" +
            $"Numéro: {pompe.Numero}\n" +
            $"Statut: {pompe.Statut}\n" +
            $"Réservoir: {pompe.ReservoirNumero ?? "Non assigné"}\n" +
            $"Compteur électronique: {pompe.CompteurElectroniqueActuel:N2} L\n" +
            $"Compteur Mécanique: {pompe.CompteurMecaniqueActuel:N2} L\n" +
            $"Écart compteurs: {discrepancy:N2} L\n" +
            $"Date création: {pompe.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A"}");
    }

    public async Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupAsync(AchatDto achat)
    {
        // Check allocation status first
        var status = await _achatAllocationService.CheckAchatAllocationStatusAsync(achat.ID);
        
        if (status == null)
        {
            await ShowAlertAsync("Erreur", "Impossible de vérifier le statut de l'allocation");
            return null;
        }

        if (!status.EstCarburant)
        {
            await ShowAlertAsync("Information", 
                "Cet achat concerne un produit non-carburant.\n" +
                "Le stock a été automatiquement mis à jour lors de la création de l'achat.");
            return null;
        }

        if (status.EstCompletementAlloue)
        {
            await ShowAlertAsync("Information", 
                $"Cet achat a déjà été entièrement alloué.\n" +
                $"Total alloué: {status.TotalAlloue:N0} L");
            return null;
        }

        var quantiteRestante = status.Restant;
        if (quantiteRestante <= 0)
        {
            await ShowAlertAsync("Information", "Aucune quantité restante à allouer.");
            return null;
        }

        var popup = new AchatAllocationPopup(_achatAllocationService, achat, quantiteRestante);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as BatchAllocationResponseDto;
    }

    public async Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupForNewAchatAsync(AchatDto achat)
    {
        // For new achats, we don't need to check allocation status - use full quantity
        if (!achat.Quantite.HasValue || achat.Quantite <= 0)
        {
            await ShowAlertAsync("Information", "Quantité non spécifiée pour cet achat.");
            return null;
        }

        // Check if it's a fuel product by checking allocation status
        var status = await _achatAllocationService.CheckAchatAllocationStatusAsync(achat.ID);
        
        if (status == null)
        {
            await ShowAlertAsync("Erreur", "Impossible de vérifier le type de produit");
            return null;
        }

        if (!status.EstCarburant)
        {
            // Non-fuel product - no allocation needed
            return null;
        }

        var quantiteToAllocate = achat.Quantite.Value;

        var popup = new AchatAllocationPopup(_achatAllocationService, achat, quantiteToAllocate);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as BatchAllocationResponseDto;
    }

    public async Task<BatchAllocationResponseDto?> ClearAndShowAllocationPopupAsync(AchatDto achat)
    {
        // First check if it's a fuel product
        var status = await _achatAllocationService.CheckAchatAllocationStatusAsync(achat.ID);
        
        if (status == null)
        {
            await ShowAlertAsync("Erreur", "Impossible de vérifier le type de produit");
            return null;
        }

        if (!status.EstCarburant)
        {
            // Non-fuel product - no allocation needed
            return null;
        }

        // Clear existing allocations
        if (status.TotalAlloue > 0)
        {
            var clearResult = await _achatAllocationService.ClearAllocationsByAchatAsync(achat.ID);
            
            if (!clearResult.Success && clearResult.CancelledCount == 0)
            {
                // All allocations failed to clear (stock consumed)
                await ShowAlertAsync("Attention", 
                    $"Impossible d'annuler les allocations existantes:\n{clearResult.Message}\n\n" +
                    "Le stock a déjà été consommé (vendu).");
                return null;
            }

            if (clearResult.FailedCount > 0)
            {
                // Partial success
                await ShowAlertAsync("Attention", 
                    $"{clearResult.CancelledCount} allocation(s) annulée(s).\n" +
                    $"{clearResult.FailedCount} allocation(s) n'ont pas pu être annulées (stock consommé).\n\n" +
                    "Vous pouvez allouer la quantité restante.");
            }
        }

        // Show allocation popup with full quantity
        var quantiteToAllocate = achat.Quantite ?? 0;
        if (quantiteToAllocate <= 0)
        {
            await ShowAlertAsync("Information", "Quantité non spécifiée pour cet achat.");
            return null;
        }

        var popup = new AchatAllocationPopup(_achatAllocationService, achat, quantiteToAllocate);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as BatchAllocationResponseDto;
    }

    public async Task ShowClientCreditManagementAsync(ClientDto client)
    {
        // Navigate to the credit management page with clientId as query parameter
        await Shell.Current.GoToAsync($"{nameof(ClientCreditManagement)}?clientId={client.ID}");
    }

    public async Task ShowAchatPerProductPopupAsync(ProductCardModel product, List<AchatAnalyticsRowDto> achats)
    {
        var popup = new AchatPerProductPopup(product, achats);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task ShowVentePerProductPopupAsync(ProductCardModel product, List<VenteAnalyticsRowDto> ventes)
    {
        var popup = new VentePerProductPopup(product, ventes);
        await CurrentPage.ShowPopupAsync(popup);
    }

    public async Task ShowVenteCarburantPerProductPopupAsync(ProductCardModel product, List<VenteCarburantAnalyticsRowDto> ventes)
    {
        var popup = new VenteCarburantPerProductPopup(product, ventes);
        await CurrentPage.ShowPopupAsync(popup);
    }

    // ????????????????????????????????????????????????????????????????
    // USER POPUPS
    // ????????????????????????????????????????????????????????????????

    public async Task<UserDto?> ShowUserCreatePopupAsync()
    {
        var popup = new UserCreatePopup(_userService);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as UserDto;
    }

    public async Task<bool> ShowUserEditPopupAsync(UserDto user)
    {
        var popup = new UserEditPopup(_userService, user);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result is bool success && success;
    }

    public async Task ShowUserDetailsPopupAsync(UserDto user)
    {
        var popup = new UserDetailsPopup(user);
        await CurrentPage.ShowPopupAsync(popup);
    }

    // ????????????????????????????????????????????????????????????????
    // STATION INFO POPUPS
    // ????????????????????????????????????????????????????????????????

    public async Task<StationInfoDto?> ShowStationInfoEditPopupAsync(StationInfoDto? existingStationInfo)
    {
        var popup = new StationInfoEditPopup(existingStationInfo);
        var result = await CurrentPage.ShowPopupAsync(popup);
        return result as StationInfoDto;
    }
}
