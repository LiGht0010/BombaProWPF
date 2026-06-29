using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Achat;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.AchatSaved"/>.
/// </summary>
public sealed record AchatSavedContext(
    int      AchatId,
    int?     FournisseurId,
    int?     EmployeId,
    string?  ModePaiement,
    decimal? Cout,
    bool     IsNew);

/// <summary>
/// When a new Achat is saved with <c>ModePaiement = "Crédit"</c>, automatically
/// creates a matching <see cref="CreditFournisseurDto"/> record so the credit
/// management section is always in sync.
/// Only fires for new achats — edits are ignored to prevent duplicate credits.
/// </summary>
public sealed class AchatCreditFournisseurAutomation : IAutomation
{
    private readonly CreditFournisseurService _creditService = new();

    public string Id          => "achat.credit-fournisseur";
    public string DisplayName => "Création automatique du crédit fournisseur à l'achat";
    public AutomationTrigger Trigger => AutomationTrigger.AchatSaved;

    public async Task RunAsync(object context)
    {
        if (context is not AchatSavedContext ctx)
        {
            Debug.WriteLine($"[AchatCreditFournisseurAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        // Only create a credit on new achats; edits do not generate new credit records.
        if (!ctx.IsNew) return;

        // Only create when the purchase was made on credit.
        if (!string.Equals(ctx.ModePaiement, "Crédit", StringComparison.OrdinalIgnoreCase)) return;

        var credit = new CreditFournisseurDto
        {
            AchatId       = ctx.AchatId,
            FournisseurId = ctx.FournisseurId,
            EmployeId     = ctx.EmployeId,
            MontantTotal  = ctx.Cout,
            Statut        = "NonPayé",
            DateCredit    = DateOnly.FromDateTime(DateTime.Today),
            // NumeroCreditF is generated server-side after persist.
        };

        var result = await _creditService.CreateCreditFournisseurAsync(credit).ConfigureAwait(false);
        if (result is null)
            Debug.WriteLine($"[AchatCreditFournisseurAutomation] Failed to create CreditFournisseur for AchatId={ctx.AchatId}");
    }
}
