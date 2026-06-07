using CommunityToolkit.Mvvm.ComponentModel;
using FourniPro.Automation;
using System.Linq;

namespace FourniPro.ViewModels;

/// <summary>
/// One row in the Automations settings list, binding-friendly wrapper around an <see cref="IAutomation"/>.
/// </summary>
public sealed class AutomationToggleItem : ObservableObject
{
    private readonly AutomationSettings _settings;

    public string Id          { get; }
    public string DisplayName { get; }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(ref _isEnabled, value))
                _settings.SetEnabled(Id, value);
        }
    }

    public AutomationToggleItem(IAutomation automation, AutomationSettings settings)
    {
        _settings   = settings;
        Id          = automation.Id;
        DisplayName = automation.DisplayName;
        _isEnabled  = settings.IsEnabled(automation.Id);
    }
}

/// <summary>
/// ViewModel for the Paramètres (settings) page.
/// Exposes per-automation toggles built from <see cref="AutomationRegistry.All"/>.
/// </summary>
public sealed class ParametresViewModel : ObservableObject
{
    public IReadOnlyList<AutomationToggleItem> Automations { get; }

    public ParametresViewModel()
    {
        var settings = AutomationSettings.Instance;
        Automations = AutomationRegistry.All
            .Select(a => new AutomationToggleItem(a, settings))
            .ToList();
    }
}
