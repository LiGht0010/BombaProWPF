using FourniPro.Services;
using FourniPro.ViewModels;
using System.Diagnostics;

namespace FourniPro.Automation.Voyage;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.VoyageSubmitted"/>.
/// </summary>
public sealed record VoyageStockContext(IReadOnlyList<StockRestantItem> Items);

/// <summary>
/// Deducts remaining stock quantities from each product after a Voyage is submitted.
/// For each <see cref="StockRestantItem"/>: Produit.Stock -= QuantiteRestante.
///
/// // AutomationHook: post-submit stock deduction plugs in here
/// </summary>
public sealed class VoyageStockDeductionAutomation : IAutomation
{
    private readonly ProduitService _produitService = new();

    public string Id          => "voyage.stock-deduction";
    public string DisplayName => "Déduction du stock à la clôture du voyage";
    public AutomationTrigger Trigger => AutomationTrigger.VoyageSubmitted;

    public async Task RunAsync(object context)
    {
        if (context is not VoyageStockContext ctx)
        {
            Debug.WriteLine($"[VoyageStockDeductionAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        foreach (var item in ctx.Items)
        {
            if (item.QuantiteRestante == 0) continue;

            // Negative delta: reduce product stock by the quantity remaining in the voyage.
            await _produitService.UpdateStockAsync(item.ProduitId, -item.QuantiteRestante).ConfigureAwait(false);
        }
    }
}
