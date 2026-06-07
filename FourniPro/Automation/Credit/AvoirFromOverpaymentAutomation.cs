using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.Automation.Credit;

/// <summary>
/// Context payload for <see cref="AutomationTrigger.PaiementCreditSaved"/>.
/// </summary>
public sealed record PaiementCreditSavedContext(
    int     CreditId,
    int     ClientId,
    decimal MontantTotal);

/// <summary>
/// Creates an Avoir for the surplus amount when the total payments on a Crédit
/// exceed the credit's original MontantTotal (trop-perçu scenario).
/// </summary>
public sealed class AvoirFromOverpaymentAutomation : IAutomation
{
    private readonly PaiementCreditService _paiementService = new();
    private readonly AvoirService          _avoirService    = new();

    public string Id          => "credit.avoir-from-overpayment";
    public string DisplayName => "Création d'avoir pour trop-perçu sur crédit";
    public AutomationTrigger Trigger => AutomationTrigger.PaiementCreditSaved;

    public async Task RunAsync(object context)
    {
        if (context is not PaiementCreditSavedContext ctx)
        {
            Debug.WriteLine($"[AvoirFromOverpaymentAutomation] Unexpected context type: {context?.GetType()}");
            return;
        }

        // Sum all payments made against this credit.
        var paiements = await _paiementService.GetAllAsync(creditId: ctx.CreditId).ConfigureAwait(false);
        var totalPaid = paiements.Sum(p => p.Montant ?? 0m);

        var surplus = totalPaid - ctx.MontantTotal;
        if (surplus <= 0m) return;

        // Idempotency: skip if an avoir for exactly this surplus already exists.
        var existing = await _avoirService.GetAllAsync(creditId: ctx.CreditId).ConfigureAwait(false);
        var duplicate = existing.FirstOrDefault(a =>
            a.Raison == "TropPercu" &&
            a.MontantAvoir == surplus);

        if (duplicate is not null) return;

        var avoir = new AvoirDto
        {
            CreditId     = ctx.CreditId,
            ClientId     = ctx.ClientId,
            MontantAvoir = surplus,
            Raison       = "TropPercu",
            DateAvoir    = DateOnly.FromDateTime(DateTime.Today),
            NumeroAvoir  = GenerateNumeroAvoir(),
        };

        var result = await _avoirService.CreateAsync(avoir).ConfigureAwait(false);
        if (result is null)
            Debug.WriteLine($"[AvoirFromOverpaymentAutomation] Failed to create avoir for CreditId={ctx.CreditId}");
    }

    private static string GenerateNumeroAvoir() =>
        $"AVR-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
}
