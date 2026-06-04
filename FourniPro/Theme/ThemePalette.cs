using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FourniPro.Theme;

/// <summary>
/// Centralized neumorphic palette swap. Overwrites palette Color keys and brush
/// instances on <see cref="Application.Current"/> so every DynamicResource brush
/// follows the active theme. Also swaps the neumorphic style dictionary so that
/// DropShadowEffect instances pick up the correct shadow colors.
/// </summary>
internal static class ThemePalette
{
    private static readonly Uri LightDictUri =
        new("pack://application:,,,/Styles/Neumorphic.Light.xaml", UriKind.Absolute);
    private static readonly Uri DarkDictUri =
        new("pack://application:,,,/Styles/Neumorphic.Dark.xaml", UriKind.Absolute);

    private static readonly Uri LightShadowDictUri =
        new("pack://application:,,,/Styles/Shadows.Light.xaml", UriKind.Absolute);
    private static readonly Uri DarkShadowDictUri =
        new("pack://application:,,,/Styles/Shadows.Dark.xaml", UriKind.Absolute);

    private static readonly Dictionary<string, Color> Light = new()
    {
        ["NeuBackgroundColor"]       = (Color)ColorConverter.ConvertFromString("#E6ECF3")!,
        ["NeuLightShadowColor"]      = (Color)ColorConverter.ConvertFromString("#FFFFFF")!,
        ["NeuDarkShadowColor"]       = (Color)ColorConverter.ConvertFromString("#B8C4D6")!,
        ["NeuAccentColor"]           = (Color)ColorConverter.ConvertFromString("#3FA89F")!,
        ["NeuAccentHoverColor"]      = (Color)ColorConverter.ConvertFromString("#52BCB3")!,
        ["NeuTextPrimaryColor"]      = (Color)ColorConverter.ConvertFromString("#2D3748")!,
        ["NeuTextSecondaryColor"]    = (Color)ColorConverter.ConvertFromString("#6B7A8F")!,
        ["NeuInputFillColor"]        = (Color)ColorConverter.ConvertFromString("#D6DEE8")!,
        ["NeuAccentSecondaryColor"]  = (Color)ColorConverter.ConvertFromString("#7B5BD3")!,
        ["NeuWarnColor"]             = (Color)ColorConverter.ConvertFromString("#D8A23A")!,
        ["NeuDangerColor"]           = (Color)ColorConverter.ConvertFromString("#D86070")!,
        ["NeuAccentGradientStart"]   = (Color)ColorConverter.ConvertFromString("#52BCB3")!,
        ["NeuAccentGradientEnd"]     = (Color)ColorConverter.ConvertFromString("#7B5BD3")!,
        ["NeuJaugAccentColor"]       = (Color)ColorConverter.ConvertFromString("#1F8C5F")!,
        ["NeuJaugAccentHoverColor"]  = (Color)ColorConverter.ConvertFromString("#176E4A")!,
    };

    private static readonly Dictionary<string, Color> Dark = new()
    {
        ["NeuBackgroundColor"]       = (Color)ColorConverter.ConvertFromString("#0B1223")!,
        ["NeuLightShadowColor"]      = (Color)ColorConverter.ConvertFromString("#121D38")!,
        ["NeuDarkShadowColor"]       = (Color)ColorConverter.ConvertFromString("#04070E")!,
        ["NeuAccentColor"]           = (Color)ColorConverter.ConvertFromString("#FF8A3D")!,
        ["NeuAccentHoverColor"]      = (Color)ColorConverter.ConvertFromString("#FFA464")!,
        ["NeuTextPrimaryColor"]      = (Color)ColorConverter.ConvertFromString("#E8ECF3")!,
        ["NeuTextSecondaryColor"]    = (Color)ColorConverter.ConvertFromString("#9AA5BA")!,
        ["NeuInputFillColor"]        = (Color)ColorConverter.ConvertFromString("#121D38")!,
        ["NeuAccentSecondaryColor"]  = (Color)ColorConverter.ConvertFromString("#B721FF")!,
        ["NeuWarnColor"]             = (Color)ColorConverter.ConvertFromString("#F5C24A")!,
        ["NeuDangerColor"]           = (Color)ColorConverter.ConvertFromString("#FF7A85")!,
        ["NeuAccentGradientStart"]   = (Color)ColorConverter.ConvertFromString("#21D4FD")!,
        ["NeuAccentGradientEnd"]     = (Color)ColorConverter.ConvertFromString("#B721FF")!,
        ["NeuJaugAccentColor"]       = (Color)ColorConverter.ConvertFromString("#2BA876")!,
        ["NeuJaugAccentHoverColor"]  = (Color)ColorConverter.ConvertFromString("#34C68C")!,
    };

    private static readonly (string BrushKey, string ColorKey)[] BrushBindings =
    {
        ("NeuBackgroundBrush",      "NeuBackgroundColor"),
        ("NeuAccentBrush",          "NeuAccentColor"),
        ("NeuAccentHoverBrush",     "NeuAccentHoverColor"),
        ("NeuTextPrimaryBrush",     "NeuTextPrimaryColor"),
        ("NeuTextSecondaryBrush",   "NeuTextSecondaryColor"),
        ("NeuInputFillBrush",       "NeuInputFillColor"),
        ("NeuAccentSecondaryBrush", "NeuAccentSecondaryColor"),
        ("NeuWarnBrush",            "NeuWarnColor"),
        ("NeuDangerBrush",          "NeuDangerColor"),
        ("NeuJaugAccentBrush",      "NeuJaugAccentColor"),
        ("NeuJaugAccentHoverBrush", "NeuJaugAccentHoverColor"),
    };

    public static void Apply(bool dark)
    {
        var palette = dark ? Dark : Light;
        var resources = Application.Current.Resources;

        foreach (var (key, color) in palette)
            resources[key] = color;

        foreach (var (brushKey, colorKey) in BrushBindings)
        {
            var brush = new SolidColorBrush(palette[colorKey]);
            brush.Freeze();
            resources[brushKey] = brush;
        }

        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
        };
        gradient.GradientStops.Add(new GradientStop(palette["NeuAccentGradientStart"], 0));
        gradient.GradientStops.Add(new GradientStop(palette["NeuAccentGradientEnd"], 1));
        gradient.Freeze();
        resources["NeuAccentGradientBrush"] = gradient;

        SwapShadowDictionary(dark ? DarkShadowDictUri : LightShadowDictUri);
        SwapNeumorphicDictionary(dark ? DarkDictUri : LightDictUri);
    }

    private static void SwapNeumorphicDictionary(Uri incomingUri)
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d =>
            d.Source == LightDictUri || d.Source == DarkDictUri);
        int insertAt = existing is not null ? merged.IndexOf(existing) : merged.Count;
        if (existing is not null) merged.Remove(existing);
        merged.Insert(insertAt, new ResourceDictionary { Source = incomingUri });
    }

    private static void SwapShadowDictionary(Uri incomingUri)
    {
        var merged = Application.Current.Resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d =>
            d.Source == LightShadowDictUri || d.Source == DarkShadowDictUri);
        int insertAt = existing is not null ? merged.IndexOf(existing) : merged.Count;
        if (existing is not null) merged.Remove(existing);
        merged.Insert(insertAt, new ResourceDictionary { Source = incomingUri });
    }
}
