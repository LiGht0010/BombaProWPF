using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace BombaProMaxWPF.Theme;

/// <summary>
/// Centralized neumorphic palette swap. Overwrites the eight palette
/// <see cref="Color"/> keys on <see cref="Application.Current"/> so every
/// brush that resolves them through <c>DynamicResource</c> follows the
/// active theme — including <c>FluentWindow.Background</c>, which
/// <c>Wpf.Ui.Appearance.ApplicationThemeManager</c> would otherwise
/// overwrite.
/// </summary>
internal static class ThemePalette
{
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
    };

    private static readonly Dictionary<string, Color> Dark = new()
    {
        ["NeuBackgroundColor"]       = (Color)ColorConverter.ConvertFromString("#2E3033")!,
        ["NeuLightShadowColor"]      = (Color)ColorConverter.ConvertFromString("#4E5358")!,
        ["NeuDarkShadowColor"]       = (Color)ColorConverter.ConvertFromString("#0F1012")!,
        ["NeuAccentColor"]           = (Color)ColorConverter.ConvertFromString("#FF8A3D")!,
        ["NeuAccentHoverColor"]      = (Color)ColorConverter.ConvertFromString("#FFA464")!,
        ["NeuTextPrimaryColor"]      = (Color)ColorConverter.ConvertFromString("#E8ECF3")!,
        ["NeuTextSecondaryColor"]    = (Color)ColorConverter.ConvertFromString("#9AA5BA")!,
        ["NeuInputFillColor"]        = (Color)ColorConverter.ConvertFromString("#26282B")!,
        // Dashboard accents (kept for the Forecourt Overview demo).
        ["NeuAccentSecondaryColor"]  = (Color)ColorConverter.ConvertFromString("#B721FF")!,
        ["NeuWarnColor"]             = (Color)ColorConverter.ConvertFromString("#F5C24A")!,
        ["NeuDangerColor"]           = (Color)ColorConverter.ConvertFromString("#FF7A85")!,
        ["NeuAccentGradientStart"]   = (Color)ColorConverter.ConvertFromString("#21D4FD")!,
        ["NeuAccentGradientEnd"]     = (Color)ColorConverter.ConvertFromString("#B721FF")!,
    };

    /// <summary>
    /// Maps each <c>Neu*Brush</c> resource key to the <c>Neu*Color</c> that
    /// drives it. Keep in sync with <c>Styles/Neumorphic.Light.xaml</c>.
    /// </summary>
    private static readonly (string BrushKey, string ColorKey)[] BrushBindings =
    {
        ("NeuBackgroundBrush",    "NeuBackgroundColor"),
        ("NeuAccentBrush",        "NeuAccentColor"),
        ("NeuAccentHoverBrush",   "NeuAccentHoverColor"),
        ("NeuTextPrimaryBrush",     "NeuTextPrimaryColor"),
        ("NeuTextSecondaryBrush",   "NeuTextSecondaryColor"),
        ("NeuInputFillBrush",       "NeuInputFillColor"),
        ("NeuAccentSecondaryBrush", "NeuAccentSecondaryColor"),
        ("NeuWarnBrush",            "NeuWarnColor"),
        ("NeuDangerBrush",          "NeuDangerColor"),
    };

    public static void Apply(bool dark)
    {
        var palette = dark ? Dark : Light;
        var resources = Application.Current.Resources;

        // 1. Overwrite Color keys (consumers via DropShadowEffect.Color etc.).
        foreach (var (key, color) in palette)
        {
            resources[key] = color;
        }

        // 2. Replace each SolidColorBrush instance entirely. Brushes used in
        //    Style setters get auto-frozen by WPF, so mutating their Color DP
        //    is silently ignored — we must swap the brush object itself.
        foreach (var (brushKey, colorKey) in BrushBindings)
        {
            var brush = new SolidColorBrush(palette[colorKey]);
            brush.Freeze();
            resources[brushKey] = brush;
        }

        // 3. Build the cyan→violet gradient brush used by hero CTAs and ring
        //    progress arcs. Frozen to keep it safe to reference from styles.
        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
        };
        gradient.GradientStops.Add(new GradientStop(palette["NeuAccentGradientStart"], 0));
        gradient.GradientStops.Add(new GradientStop(palette["NeuAccentGradientEnd"], 1));
        gradient.Freeze();
        resources["NeuAccentGradientBrush"] = gradient;
    }
}
