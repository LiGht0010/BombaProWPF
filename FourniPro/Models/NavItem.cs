using System.ComponentModel;
using FourniPro.Localization;
using Wpf.Ui.Controls;

namespace FourniPro.Models;

/// <summary>
/// Represents a single entry in the shell's left navigation sidebar.
/// </summary>
public sealed class NavItem : INotifyPropertyChanged
{
    public NavItem(string key, Func<string> titleAccessor, SymbolRegular icon)
    {
        Key = key;
        TitleAccessor = titleAccessor;
        Icon = icon;

        LanguageManager.Instance.LanguageChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
    }

    /// <summary>Stable identifier used for selection / lookup.</summary>
    public string Key { get; }

    /// <summary>Display label resolved through an accessor to follow the active language.</summary>
    public Func<string> TitleAccessor { get; }

    public string Title => TitleAccessor();

    public SymbolRegular Icon { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
