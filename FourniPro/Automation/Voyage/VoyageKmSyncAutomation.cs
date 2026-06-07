using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Voyage;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.VoyageStatusChanged"/>.
/// </summary>
public sealed record VoyageStatusContext(
    int     CamionId,
    string  NewStatus,
    int?    KilometrageFinal);

/// <summary>
/// Syncs the camion's Kilometrage to the voyage's KilometrageFinal
/// when a voyage is marked as Completed.
/// </summary>
public sealed class VoyageKmSyncAutomation : IAutomation
{
    private readonly CamionService _camionService = new();

    public string Id          => "voyage.km-sync";
    public string DisplayName => "Mise à jour du kilométrage du camion à la clôture";
    public AutomationTrigger Trigger => AutomationTrigger.VoyageStatusChanged;

    public async Task RunAsync(object context)
    {
        if (context is not VoyageStatusContext ctx)
        {
            Debug.WriteLine($"[VoyageKmSyncAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        if (ctx.NewStatus != "Completed") return;
        if (ctx.KilometrageFinal is null) return;

        var camion = await _camionService.GetCamionByIdAsync(ctx.CamionId).ConfigureAwait(false);
        if (camion is null)
        {
            Debug.WriteLine($"[VoyageKmSyncAutomation] Camion {ctx.CamionId} not found.");
            return;
        }

        camion.Kilometrage = ctx.KilometrageFinal.Value;
        await _camionService.UpdateCamionAsync(camion).ConfigureAwait(false);
    }
}
