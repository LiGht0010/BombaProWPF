namespace FourniPro.Automation;

/// <summary>
/// Contract for every pluggable automation step.
/// </summary>
public interface IAutomation
{
    /// <summary>Unique, stable identifier used to persist enabled/disabled state.</summary>
    string Id { get; }

    /// <summary>Human-readable name shown in the Settings page.</summary>
    string DisplayName { get; }

    /// <summary>The event that causes this automation to run.</summary>
    AutomationTrigger Trigger { get; }

    /// <summary>
    /// Executes the automation logic.
    /// </summary>
    /// <param name="context">
    /// Trigger-specific payload — see each <see cref="AutomationTrigger"/> value for the expected type.
    /// </param>
    Task RunAsync(object context);
}
