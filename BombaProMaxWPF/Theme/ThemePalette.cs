using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BombaProMaxWPF.Theme;

/// <summary>
/// Centralized neumorphic palette swap. Overwrites palette
/// <see cref="Color"/> keys and brush instances on <see cref="Application.Current"/>
/// so every brush that resolves them through <c>DynamicResource</c> follows
/// the active theme. Also swaps the neumorphic style dictionary entirely so
/// that <see cref="System.Windows.Media.Effects.DropShadowEffect"/> instances
/// — which WPF freezes at parse time and never re-evaluates via DynamicResource
/// — pick up the hardcoded shadow colors baked into the correct file.
/// </summary>
internal static class ThemePalette
{
    // Pack URIs for the two neumorphic style dictionaries. These must match
    // the Source paths declared in App.xaml (relative to the assembly root).
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
        // Dashboard accents (kept for the Forecourt Overview demo).
        ["NeuAccentSecondaryColor"]  = (Color)ColorConverter.ConvertFromString("#B721FF")!,
        ["NeuWarnColor"]             = (Color)ColorConverter.ConvertFromString("#F5C24A")!,
        ["NeuDangerColor"]           = (Color)ColorConverter.ConvertFromString("#FF7A85")!,
        ["NeuAccentGradientStart"]   = (Color)ColorConverter.ConvertFromString("#21D4FD")!,
        ["NeuAccentGradientEnd"]     = (Color)ColorConverter.ConvertFromString("#B721FF")!,
        ["NeuJaugAccentColor"]       = (Color)ColorConverter.ConvertFromString("#2BA876")!,
        ["NeuJaugAccentHoverColor"]  = (Color)ColorConverter.ConvertFromString("#34C68C")!,
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
        ("NeuJaugAccentBrush",      "NeuJaugAccentColor"),
        ("NeuJaugAccentHoverBrush", "NeuJaugAccentHoverColor"),
    };

    public static void Apply(bool dark)
    {
        var palette = dark ? Dark : Light;
        var resources = Application.Current.Resources;

        // 1. Overwrite Color keys.
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

        // 4. Swap the neumorphic style dictionary LAST so DropShadowEffect
        //    instances (frozen at parse time) pick up their hardcoded shadow
        //    colors from the correct file, and the already-written brush
        //    resources are in place before any template is re-parsed.
        SwapNeumorphicDictionary(dark ? DarkDictUri : LightDictUri);
        SwapShadowDictionary(dark ? DarkShadowDictUri : LightShadowDictUri);
    }

    /// <summary>
    /// Finds whichever neumorphic dictionary is currently in
    /// <see cref="Application.Current"/> merged dictionaries, removes it,
    /// then inserts <paramref name="incomingUri"/> at the same position.
    /// Inserting at the original position preserves key-override order
    /// (SegmentedPill.xaml and wpf-ui dicts stay in the correct slots).
    /// </summary>
    private static void SwapNeumorphicDictionary(Uri incomingUri)
    {
        var merged = Application.Current.Resources.MergedDictionaries;

        // Find the currently loaded neumorphic dictionary by its source URI.
        var existing = merged.FirstOrDefault(d =>
            d.Source == LightDictUri || d.Source == DarkDictUri);

        // Always remove and re-insert — even when the URI matches — so that
        // DropShadowEffect instances (frozen at parse time) are re-created
        // with the correct shadow colors and live controls get re-resolved.
        int insertAt = existing is not null ? merged.IndexOf(existing) : merged.Count;

        // Remove old dictionary first — this invalidates all styles that were
        // parsed from it, which is intentional: WPF will re-resolve them from
        // the new dictionary on the next layout pass.
        if (existing is not null)
            merged.Remove(existing);

        // Insert the incoming dictionary at the same slot.
        merged.Insert(insertAt, new ResourceDictionary { Source = incomingUri });
    }

    /// <summary>
    /// Same pattern as <see cref="SwapNeumorphicDictionary"/> but targets the
    /// shadow-effect dictionary pair (<c>Shadows.Light.xaml</c> /
    /// <c>Shadows.Dark.xaml</c>). Must be called after the main neumorphic
    /// dictionary swap so the new <c>DropShadowEffect</c> resources are available
    /// to the freshly re-parsed styles.
    /// </summary>
    private static void SwapShadowDictionary(Uri incomingUri)
    {
        var merged = Application.Current.Resources.MergedDictionaries;

        var existing = merged.FirstOrDefault(d =>
            d.Source == LightShadowDictUri || d.Source == DarkShadowDictUri);

        int insertAt = existing is not null ? merged.IndexOf(existing) : merged.Count;

        if (existing is not null)
            merged.Remove(existing);

        merged.Insert(insertAt, new ResourceDictionary { Source = incomingUri });
    }
}
