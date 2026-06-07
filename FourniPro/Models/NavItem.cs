using System.ComponentModel;
using FourniPro.Localization;
using Wpf.Ui.Controls;

namespace FourniPro.Models;

/// <summary>
/// Represents a single entry in the shell's left navigation sidebar.
/// </summary>
public sealed class NavItem : INotifyPropertyChanged
{
    public NavItem(string key, Func<string> titleAccessor, SymbolRegular icon, bool isEnabled = true, Func<string>? tooltipAccessor = null)
    {
        Key = key;
        TitleAccessor = titleAccessor;
        TooltipAccessor = tooltipAccessor;
        Icon = icon;
        IsEnabled = isEnabled;

        LanguageManager.Instance.LanguageChanged += (_, _) =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tooltip)));
        };
    }

    /// <summary>Stable identifier used for selection / lookup.</summary>
    public string Key { get; }

    /// <summary>Display label resolved through an accessor to follow the active language.</summary>
    public Func<string> TitleAccessor { get; }

    public string Title => TitleAccessor();

    /// <summary>Tooltip resolved through an accessor to follow the active language. Null when not set.</summary>
    public Func<string>? TooltipAccessor { get; }

    public string? Tooltip => TooltipAccessor?.Invoke();

    public SymbolRegular Icon { get; }

    /// <summary>False disables the sidebar item (stub pages not yet implemented).</summary>
    public bool IsEnabled { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
