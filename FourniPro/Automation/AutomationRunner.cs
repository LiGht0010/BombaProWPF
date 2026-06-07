using System.Diagnostics;

namespace FourniPro.Automation;

/// <summary>
/// Executes all enabled automations registered for a given <see cref="AutomationTrigger"/>.
/// Instantiate once and share (e.g., as a singleton field in each ViewModel that needs it).
/// </summary>
public sealed class AutomationRunner
{
    private readonly AutomationSettings _settings;

    public AutomationRunner() : this(AutomationSettings.Instance) { }

    /// <summary>Constructor for testing — supply an isolated <see cref="AutomationSettings"/> instance.</summary>
    public AutomationRunner(AutomationSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Runs every automation that matches <paramref name="trigger"/> and is currently enabled.
    /// Exceptions thrown by individual automations are caught, logged, and do not prevent remaining automations from running.
    /// </summary>
    /// <param name="trigger">The event that occurred.</param>
    /// <param name="context">Trigger-specific payload.</param>
    public async Task RunAsync(AutomationTrigger trigger, object context)
    {
        foreach (var automation in AutomationRegistry.All)
        {
            if (automation.Trigger != trigger) continue;
            if (!_settings.IsEnabled(automation.Id)) continue;

            try
            {
                await automation.RunAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutomationRunner] '{automation.Id}' failed: {ex.Message}");
            }
        }
    }
}
