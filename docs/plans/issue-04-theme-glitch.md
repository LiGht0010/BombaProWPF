# Issue #4 — Theme toggle background glitch

## What user reported
> "the background glitches… it stays dark for both of the themes only the grids keeps switching"

After a couple of theme toggles the **window background** sticks on dark while the inner cards/grids continue to swap correctly between light and dark.

## Diagnosis
**Where it lives:**
- `BombaProMaxWPF/MainWindow.xaml.cs` → `ApplyNeumorphicPalette`, `ThemeToggle_Checked`, `ThemeToggle_Unchecked`.
- `BombaProMaxWPF/MainWindow.xaml` line 16 (`Background="{DynamicResource NeuBackgroundBrush}"`) and the `ui:TitleBar` `Background` (lines 28–29).
- `BombaProMaxWPF/Styles/Neumorphic.Light.xaml` and `Neumorphic.Dark.xaml` (define `NeuBackgroundColor`/`NeuBackgroundBrush` as static within each dictionary).

**Why it happens (two compounding bugs):**

1. **Dictionary swap fights Wpf.Ui's theme manager.** Each toggle calls:
   ```csharp
   ApplicationThemeManager.Apply(ApplicationTheme.Dark);   // or Light
   ApplyNeumorphicPalette(dark: true);                     // remove Light dict, add Dark dict
   ```
   `ApplicationThemeManager.Apply` writes its own brushes into `Application.Current.Resources`, including the `FluentWindow.Background` it owns. After multiple swaps the Wpf.Ui-applied background wins and stays dark even when our merged dictionary now points at the Light palette.

2. **`SolidColorBrush x:Key="NeuBackgroundBrush" Color="{StaticResource NeuBackgroundColor}"`** in each dictionary uses **`StaticResource`** for the color. WPF resolves the brush once at first lookup. When we remove/add the dictionary the brush instance changes, but the `FluentWindow.Background` reference is no longer rediscovered because Wpf.Ui has already pinned its own.

**Net effect:** the inner cards reference brushes through `DynamicResource` from the live merged dictionary and follow the swap; the window itself doesn't.

## Proposed fix (apply only on GO)
Stop swapping dictionaries; swap **Color resources** at the application level instead.

- Keep `Neumorphic.Light.xaml` permanently merged in `App.xaml` (it provides every style and the brush `x:Key`s).
- Rewrite `ApplyNeumorphicPalette(bool dark)` to overwrite the 8 palette `Color` keys (`NeuBackgroundColor`, `NeuLightShadowColor`, `NeuDarkShadowColor`, `NeuAccentColor`, `NeuAccentHoverColor`, `NeuTextPrimaryColor`, `NeuTextSecondaryColor`, `NeuInputFillColor`) directly in `Application.Current.Resources`. Two static palette tables (Light + Dark) hold the values currently in each `.xaml`.
- In `Neumorphic.Light.xaml`, change every `SolidColorBrush x:Key="...Brush" Color="{StaticResource ...Color}"` to `Color="{DynamicResource ...Color}"` so brushes follow the live color override.
- `Neumorphic.Dark.xaml` becomes optional (only kept for reference) — it is no longer merged.
- Reorder `ThemeToggle_Checked/_Unchecked` to: **`ApplyNeumorphicPalette(dark)` first, then `ApplicationThemeManager.Apply(...)`**. Wpf.Ui then sees our overrides and respects them.
- Verify `MainWindow.xaml` line 16 `Background="{DynamicResource NeuBackgroundBrush}"` still works and now updates on every toggle.

This is a single-method refactor in `MainWindow.xaml.cs` plus a small `StaticResource → DynamicResource` substitution inside `Neumorphic.Light.xaml`. No structural XAML changes.

## Status
✅ Done — created `BombaProMaxWPF/Theme/ThemePalette.cs` (centralized 8-key palette swap). `MainWindow.xaml.cs` and `LoginWindow.xaml.cs` now call `ThemePalette.Apply(dark)` *before* `ApplicationThemeManager.Apply(...)` instead of removing/adding `Neumorphic.{Light,Dark}.xaml`. `Neumorphic.Light.xaml` now uses `DynamicResource` for every `Neu*Color` reference (brushes + gradient stops) so palette swaps cascade. `Neumorphic.Dark.xaml` is no longer merged at runtime (kept on disk for reference). Build green.
