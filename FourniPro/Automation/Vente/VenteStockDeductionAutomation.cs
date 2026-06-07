using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Vente;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.VenteSaved"/>.
/// </summary>
public sealed record VenteStockContext(
    int  ProduitId,
    int  Quantite,
    int? VoyageId);

/// <summary>
/// Deducts sold quantity from the product's stock when a standalone Vente is saved.
/// Skipped when the Vente belongs to a Voyage (stock is managed by the voyage flow instead).
/// </summary>
public sealed class VenteStockDeductionAutomation : IAutomation
{
    private readonly ProduitService _produitService = new();

    public string Id          => "vente.stock-deduction";
    public string DisplayName => "Déduction du stock à la vente (hors voyage)";
    public AutomationTrigger Trigger => AutomationTrigger.VenteSaved;

    public async Task RunAsync(object context)
    {
        if (context is not VenteStockContext ctx)
        {
            Debug.WriteLine($"[VenteStockDeductionAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        // Stock inside a voyage is handled by the voyage automation — skip.
        if (ctx.VoyageId is not null) return;

        if (ctx.Quantite <= 0) return;

        await _produitService.UpdateStockAsync(ctx.ProduitId, -ctx.Quantite).ConfigureAwait(false);
    }
}
