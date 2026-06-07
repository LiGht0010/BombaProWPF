using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Vente;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.VenteDeleted"/>.
/// </summary>
public sealed record VenteDeletedContext(
    int     VenteId,
    int     ClientId,
    int     ProduitId,
    int     Quantite,
    decimal PrixUnitaire,
    decimal MontantTotal);

/// <summary>
/// Creates an Avoir for the full sale amount when a Vente is deleted.
/// </summary>
public sealed class AvoirFromVenteDeletionAutomation : IAutomation
{
    private readonly AvoirService _avoirService = new();

    public string Id          => "vente.avoir-from-deletion";
    public string DisplayName => "Création d'avoir à la suppression d'une vente";
    public AutomationTrigger Trigger => AutomationTrigger.VenteDeleted;

    public async Task RunAsync(object context)
    {
        if (context is not VenteDeletedContext ctx)
        {
            Debug.WriteLine($"[AvoirFromVenteDeletionAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        var avoir = new AvoirDto
        {
            VenteId      = ctx.VenteId,
            ClientId     = ctx.ClientId,
            ProduitId    = ctx.ProduitId,
            Quantite     = ctx.Quantite,
            PrixUnitaire = ctx.PrixUnitaire,
            MontantAvoir = ctx.MontantTotal,
            Raison       = "Annulation",
            DateAvoir    = DateOnly.FromDateTime(DateTime.Today),
            NumeroAvoir  = GenerateNumeroAvoir(),
        };

        var result = await _avoirService.CreateAsync(avoir).ConfigureAwait(false);
        if (result is null)
            Debug.WriteLine($"[AvoirFromVenteDeletionAutomation] Failed to create avoir for VenteId={ctx.VenteId}");
    }

    private static string GenerateNumeroAvoir() =>
        $"AVR-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
}
