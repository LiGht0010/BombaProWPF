namespace FourniPro.Automation;

/// <summary>
/// Identifies the application event that triggers an automation.
/// </summary>
public enum AutomationTrigger
{
    /// <summary>
    /// Fired when an Achat (purchase) is created or updated and its product / quantity changes.
    /// Context type: <see cref="Achat.AchatStockContext"/>.
    /// </summary>
    AchatProduitChanged,

    /// <summary>
    /// Fired after a Voyage is fully submitted (all stock + transactions persisted).
    /// Context type: <see cref="Voyage.VoyageStockContext"/>.
    /// </summary>
    VoyageSubmitted,

    /// <summary>
    /// Fired when a Voyage header is saved with a changed Statut.
    /// Context type: <see cref="Voyage.VoyageStatusContext"/>.
    /// </summary>
    VoyageStatusChanged,

    /// <summary>
    /// Fired after a standalone Vente is created or updated (not inside a Voyage dialog).
    /// Context type: <see cref="Vente.VenteStockContext"/>.
    /// </summary>
    VenteSaved,

    /// <summary>
    /// Fired after a standalone Crédit is created or updated (not inside a Voyage dialog).
    /// Context type: <see cref="Credit.CreditStockContext"/>.
    /// </summary>
    CreditSaved,

    /// <summary>
    /// Fired after a PaiementCredit is created or updated.
    /// Context type: <see cref="Credit.PaiementCreditSavedContext"/>.
    /// </summary>
    PaiementCreditSaved,

    /// <summary>
    /// Fired after a standalone Vente is deleted (not inside a Voyage dialog).
    /// Context type: <see cref="Vente.VenteDeletedContext"/>.
    /// </summary>
    VenteDeleted,

    /// <summary>
    /// Fired after an Achat is created or updated.
    /// Context type: <see cref="Achat.AchatSavedContext"/>.
    /// </summary>
    AchatSaved,
}
