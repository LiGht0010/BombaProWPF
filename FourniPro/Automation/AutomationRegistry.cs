using FourniPro.Automation.Achat;
using FourniPro.Automation.Credit;
using FourniPro.Automation.Vente;
using FourniPro.Automation.Voyage;

namespace FourniPro.Automation;

/// <summary>
/// Central list of all registered <see cref="IAutomation"/> instances.
/// Add new automations here — no reflection, explicit registration only.
/// </summary>
public static class AutomationRegistry
{
    /// <summary>All registered automations. Populated at class-initialization time.</summary>
    public static IReadOnlyList<IAutomation> All { get; } = Build();

    private static List<IAutomation> Build() =>
    [
        new AchatStockUpdateAutomation(),
        new AchatCreditFournisseurAutomation(),
        new VoyageStockDeductionAutomation(),
        new VoyageKmSyncAutomation(),
        new VenteStockDeductionAutomation(),
        new CreditStockDeductionAutomation(),
        new AvoirFromOverpaymentAutomation(),
        new AvoirFromVenteDeletionAutomation(),
    ];
}
