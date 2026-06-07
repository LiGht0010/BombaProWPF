using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Credit;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.CreditSaved"/>.
/// </summary>
public sealed record CreditStockContext(
    int  ProduitId,
    int  Quantite,
    int? VoyageId);

/// <summary>
/// Deducts credited quantity from the product's stock when a standalone Crédit is saved.
/// Skipped when the Crédit belongs to a Voyage (stock is managed by the voyage flow instead).
/// </summary>
public sealed class CreditStockDeductionAutomation : IAutomation
{
    private readonly ProduitService _produitService = new();

    public string Id          => "credit.stock-deduction";
    public string DisplayName => "Déduction du stock au crédit (hors voyage)";
    public AutomationTrigger Trigger => AutomationTrigger.CreditSaved;

    public async Task RunAsync(object context)
    {
        if (context is not CreditStockContext ctx)
        {
            Debug.WriteLine($"[CreditStockDeductionAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        // Stock inside a voyage is handled by the voyage automation — skip.
        if (ctx.VoyageId is not null) return;

        if (ctx.Quantite <= 0) return;

        await _produitService.UpdateStockAsync(ctx.ProduitId, -ctx.Quantite).ConfigureAwait(false);
    }
}
