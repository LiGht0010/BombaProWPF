# Issue #4 (redo) — Theme toggle: cards stay light, window background sticks black

## What user reported (after first attempt)
> Light start ✓. Switch to dark → only window background goes black; cards/sidebar/toolbar stay light. Switch back to light → cards already light, but window background stays black.

Screenshot confirmed: cards keep their light palette regardless of toggle; the FluentWindow paints a near-black background that doesn't follow our Light/Dark palette.

## Why the first fix wasn't enough
The previous fix (`ThemePalette.Apply` rewriting only the eight `Neu*Color` keys) assumed every `Neu*Brush` would re-resolve through `DynamicResource` and pick up the new color. Two things broke that:

1. **`SolidColorBrush` resources auto-freeze.** The brushes in `Styles/Neumorphic.Light.xaml` are declared as `<SolidColorBrush x:Key="NeuBackgroundBrush" Color="{DynamicResource NeuBackgroundColor}" />`. Once a brush is consumed by a `Style` setter (e.g. `Border.Background`), WPF can freeze the instance. After freezing, updates to the `Color` DP are silently ignored — so swapping the underlying `NeuBackgroundColor` value has no visible effect on cards already realized.

2. **`Wpf.Ui.Appearance.ApplicationThemeManager.Apply` writes `FluentWindow.Background` as a local DP value** with its own (near-black) dark brush. A locally-set value beats a XAML `DynamicResource` markup, so our `Background="{DynamicResource NeuBackgroundBrush}"` is overridden the moment we call `ApplicationThemeManager.Apply(Dark)`. Switching back to light, Wpf.Ui's local Background stays around / re-applies its own light brush, but never re-resolves through our resource — and even if it did, the brush is frozen at the original color.

## Fix to apply
**`BombaProMaxWPF/Theme/ThemePalette.cs`** — extend `Apply(bool dark)` to also overwrite the six brush resources, not just the eight color keys:

- For each of `NeuBackgroundBrush`, `NeuAccentBrush`, `NeuAccentHoverBrush`, `NeuTextPrimaryBrush`, `NeuTextSecondaryBrush`, `NeuInputFillBrush`, build a fresh `SolidColorBrush` from the current palette color, freeze it for perf, and write `Application.Current.Resources[brushKey] = newBrush`.
- Color keys still go in too (for `DropShadowEffect.Color="{DynamicResource Neu*Color}"` consumers and for any element binding to `Color` directly).

**`BombaProMaxWPF/MainWindow.xaml.cs`** and **`BombaProMaxWPF/Views/LoginWindow.xaml.cs`** — after every `ApplicationThemeManager.Apply(...)`, re-install a `DynamicResource` reference on `Background` (and `Foreground`) so Wpf.Ui's local-value write is overridden:

```csharp
ApplicationThemeManager.Apply(theme);
this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
```

No XAML change required.

## Status
✅ Applied — build green; awaiting runtime validation.
