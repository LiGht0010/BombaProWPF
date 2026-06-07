using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Achat;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.AchatProduitChanged"/>.
/// </summary>
public sealed record AchatStockContext(
    int  ProduitId,
    int  OldQuantite,
    int  NewQuantite);

/// <summary>
/// Updates the product's stock when an Achat is created or its quantity changes.
/// Migrated from the inline stock-update logic that lived in
/// <c>NouveauAchatViewModel.SaveAsync</c> and <c>EditAchatViewModel.SaveAsync</c>.
/// </summary>
public sealed class AchatStockUpdateAutomation : IAutomation
{
    private readonly ProduitService _produitService = new();

    public string Id          => "achat.stock-update";
    public string DisplayName => "Mise à jour du stock à l'achat";
    public AutomationTrigger Trigger => AutomationTrigger.AchatProduitChanged;

    public async Task RunAsync(object context)
    {
        if (context is not AchatStockContext ctx)
        {
            Debug.WriteLine($"[AchatStockUpdateAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        var delta = ctx.NewQuantite - ctx.OldQuantite;
        if (delta == 0) return;

        await _produitService.UpdateStockAsync(ctx.ProduitId, delta).ConfigureAwait(false);
    }
}
