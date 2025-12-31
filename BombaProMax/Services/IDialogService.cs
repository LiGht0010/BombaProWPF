using BombaProMax.Models;
using BombaProMax.Models.Dashboard;

namespace BombaProMax.Services;

/// <summary>
/// Abstraction for dialog and popup operations to support MVVM pattern.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an alert dialog with a single button.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Shows a confirmation dialog with two buttons.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Oui", string cancel = "Non");

    /// <summary>
    /// Shows the client creation popup and returns the created client or null.
    /// </summary>
    Task<ClientDto?> ShowClientCreatePopupAsync();

    /// <summary>
    /// Shows the client edit popup and returns true if the client was updated.
    /// /// </summary>
    Task<bool> ShowClientEditPopupAsync(ClientDto client);

    /// <summary>
    /// Shows the client details popup.
    /// </summary>
    Task ShowClientDetailsPopupAsync(ClientDto client);

    /// <summary>
    /// Shows the achat creation popup and returns the created achat or null.
    /// </summary>
    Task<AchatDto?> ShowAchatCreatePopupAsync();

    /// <summary>
    /// Shows the achat edit popup and returns true if the achat was updated.
    /// </summary>
    Task<bool> ShowAchatEditPopupAsync(AchatDto achat);

    /// <summary>
    /// Shows the achat details popup.
    /// </summary>
    Task ShowAchatDetailsPopupAsync(AchatDto achat);

    /// <summary>
    /// Shows the chauffeur creation popup and returns the created chauffeur or null.
    /// </summary>
    Task<ChauffeurDto?> ShowChauffeurCreatePopupAsync();

    /// <summary>
    /// Shows the chauffeur edit popup and returns true if the chauffeur was updated.
    /// </summary>
    Task<bool> ShowChauffeurEditPopupAsync(ChauffeurDto chauffeur);

    /// <summary>
    /// Shows the chauffeur details popup.
    /// </summary>
    Task ShowChauffeurDetailsPopupAsync(ChauffeurDto chauffeur);

    /// <summary>
    /// Shows the fournisseur creation popup and returns the created fournisseur or null.
    /// </summary>
    Task<FournisseurDto?> ShowFournisseurCreatePopupAsync();

    /// <summary>
    /// Shows the fournisseur edit popup and returns true if the fournisseur was updated.
    /// </summary>
    Task<bool> ShowFournisseurEditPopupAsync(FournisseurDto fournisseur);

    /// <summary>
    /// Shows the fournisseur details popup.
    /// </summary>
    Task ShowFournisseurDetailsPopupAsync(FournisseurDto fournisseur);

    /// <summary>
    /// Shows the citerne creation popup and returns the created citerne or null.
    /// </summary>
    Task<CiterneDto?> ShowCiterneCreatePopupAsync();

    /// <summary>
    /// Shows the citerne edit popup and returns true if the citerne was updated.
    /// </summary>
    Task<bool> ShowCiterneEditPopupAsync(CiterneDto citerne);

    /// <summary>
    /// Shows the citerne details popup.
    /// </summary>
    Task ShowCiterneDetailsPopupAsync(CiterneDto citerne);

    /// <summary>
    /// Shows the camion creation popup and returns the created camion or null.
    /// </summary>
    Task<CamionDto?> ShowCamionCreatePopupAsync();

    /// <summary>
    /// Shows the camion edit popup and returns true if the camion was updated.
    /// </summary>
    Task<bool> ShowCamionEditPopupAsync(CamionDto camion);

    /// <summary>
    /// Shows the camion details popup.
    /// </summary>
    Task ShowCamionDetailsPopupAsync(CamionDto camion);

    /// <summary>
    /// Shows the produit creation popup and returns the created produit or null.
    /// </summary>
    Task<ProduitDto?> ShowProduitCreatePopupAsync();

    /// <summary>
    /// Shows the produit edit popup and returns true if the produit was updated.
    /// </summary>
    Task<bool> ShowProduitEditPopupAsync(ProduitDto produit);

    /// <summary>
    /// Shows the produit details popup.
    /// </summary>
    Task ShowProduitDetailsPopupAsync(ProduitDto produit);

    /// <summary>
    /// Shows the reservoir creation popup and returns the created reservoir or null.
    /// </summary>
    Task<ReservoirDto?> ShowReservoirCreatePopupAsync();

    /// <summary>
    /// Shows the reservoir edit popup and returns true if the reservoir was updated.
    /// </summary>
    Task<bool> ShowReservoirEditPopupAsync(ReservoirDto reservoir);

    /// <summary>
    /// Shows the reservoir details popup.
    /// </summary>
    Task ShowReservoirDetailsPopupAsync(ReservoirDto reservoir);

    /// <summary>
    /// Shows the pompe creation popup and returns the created pompe or null.
    /// </summary>
    Task<PompeDto?> ShowPompeCreatePopupAsync();

    /// <summary>
    /// Shows the pompe edit popup and returns true if the pompe was updated.
    /// </summary>
    Task<bool> ShowPompeEditPopupAsync(PompeDto pompe);

    /// <summary>
    /// Shows the pompe details popup.
    /// </summary>
    Task ShowPompeDetailsPopupAsync(PompeDto pompe);

    /// <summary>
    /// Shows the achat allocation popup for distributing fuel to reservoirs.
    /// Returns the allocation result or null if cancelled.
    /// </summary>
    Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupAsync(AchatDto achat);

    /// <summary>
    /// Shows the achat allocation popup for a newly created achat (full quantity).
    /// Skips allocation status check since it's a new achat.
    /// </summary>
    Task<BatchAllocationResponseDto?> ShowAchatAllocationPopupForNewAchatAsync(AchatDto achat);

    /// <summary>
    /// Clears existing allocations for an achat and shows the allocation popup.
    /// Used when an achat is modified and needs re-allocation.
    /// </summary>
    Task<BatchAllocationResponseDto?> ClearAndShowAllocationPopupAsync(AchatDto achat);

    /// <summary>
    /// Navigates to the client credit management page.
    /// </summary>
    Task ShowClientCreditManagementAsync(ClientDto client);

    /// <summary>
    /// Shows the achat per product popup with details table.
    /// </summary>
    Task ShowAchatPerProductPopupAsync(ProductCardModel product, List<AchatAnalyticsRowDto> achats);

    /// <summary>
    /// Shows the vente per product popup with details table.
    /// </summary>
    Task ShowVentePerProductPopupAsync(ProductCardModel product, List<VenteAnalyticsRowDto> ventes);

    /// <summary>
    /// Shows the vente carburant per product popup with dual meter details table.
    /// </summary>
    Task ShowVenteCarburantPerProductPopupAsync(ProductCardModel product, List<VenteCarburantAnalyticsRowDto> ventes);

    // ????????????????????????????????????????????????????????????????
    // USER POPUPS
    // ????????????????????????????????????????????????????????????????

    /// <summary>
    /// Shows the user creation popup and returns the created user or null.
    /// </summary>
    Task<UserDto?> ShowUserCreatePopupAsync();

    /// <summary>
    /// Shows the user edit popup and returns true if the user was updated.
    /// </summary>
    Task<bool> ShowUserEditPopupAsync(UserDto user);

    /// <summary>
    /// Shows the user details popup.
    /// </summary>
    Task ShowUserDetailsPopupAsync(UserDto user);

    /// <summary>
    /// Shows the station info edit/create popup and returns the saved station info or null.
    /// </summary>
    Task<StationInfoDto?> ShowStationInfoEditPopupAsync(StationInfoDto? existingStationInfo);
}
