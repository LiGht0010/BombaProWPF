using System;
using System.ComponentModel;
using BombaProMaxWPF.Localization;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Models;

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

        // Refresh the localized title whenever the active language changes.
        LanguageManager.Instance.LanguageChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
    }

    /// <summary>
    /// Stable identifier (used for selection / lookup).
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Display label. Resolved through an accessor so it follows the active language.
    /// </summary>
    public Func<string> TitleAccessor { get; }

    public string Title => TitleAccessor();

    public SymbolRegular Icon { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
