using System.ComponentModel;
using System.Globalization;
using System.Threading;
using FourniPro.Resources;

namespace FourniPro.Localization;

/// <summary>
/// App-wide language manager. Holds the current culture and notifies all
/// bindings when the language changes so every translated UI element refreshes.
/// </summary>
public sealed class LanguageManager : INotifyPropertyChanged
{
    public static LanguageManager Instance { get; } = new();

    private CultureInfo _culture = new("fr");

    private LanguageManager() { }

    public CultureInfo CurrentCulture => _culture;

    /// <summary>
    /// RTL for Arabic, LTR otherwise.
    /// </summary>
    public System.Windows.FlowDirection FlowDirection =>
        _culture.TextInfo.IsRightToLeft
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;

    /// <summary>
    /// XAML-friendly indexer to fetch a localized string for the current culture.
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return Strings.ResourceManager.GetString(key, _culture) ?? $"!{key}!";
        }
    }

    public void SetLanguage(string cultureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);

        var newCulture = new CultureInfo(cultureCode);
        if (string.Equals(newCulture.Name, _culture.Name, StringComparison.OrdinalIgnoreCase))
            return;

        _culture = newCulture;
        Strings.Culture = newCulture;
        Thread.CurrentThread.CurrentUICulture = newCulture;
        CultureInfo.DefaultThreadCurrentUICulture = newCulture;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlowDirection)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;
}
