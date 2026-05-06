# Issue #7 — Collapsed nav: indicator butts the icon, HoverFill offset

## What user reported
Screenshot of collapsed sidebar shows icons rendering, but:
- The selected nav row's left accent indicator sits with ~0 spacing against the icon.
- The selected row's blue rounded highlight (HoverFill) is visibly **offset to the right** of the icon — looks like a "shadow" leaking past the icon instead of centering under it.

## Diagnosis
Two hard-coded values in `NeuNavItemStyle` template (`Styles/Neumorphic.Light.xaml`):

1. **`ContentPresenter HorizontalAlignment="Stretch"`** — defeats `RadioButton.HorizontalContentAlignment = Center` set from code-behind. The DataTemplate Grid (`Auto / *`) keeps the icon glued to column 0 (left), so even with `SymbolIcon.HorizontalAlignment = Center` the icon sits at the row's left edge, right next to the 3 px Indicator.

2. **`HoverFill Margin="8,0,2,0"`** — asymmetric. The selection rectangle spans `8 px left → 2 px right`. Visual center is `(width+6)/2`, i.e. **3 px right of true row center**. Combined with #1, the highlight is offset right of the (left-glued) icon.

## Fix applied
Edited `BombaProMaxWPF/Styles/Neumorphic.Light.xaml` — `NeuNavItemStyle` template only:

- `HoverFill Margin` → `6,0,6,0` (symmetric — selection rectangle centers about the row).
- `ContentPresenter HorizontalAlignment` → `{TemplateBinding HorizontalContentAlignment}`. With `HorizontalContentAlignment=Center` (collapsed mode), the presenter shrinks to fit the icon and centers it. With `Stretch` (expanded default), label-row layout is unchanged.

No code-behind change. No `MainWindow.xaml` change. Build green.

## Status
✅ Applied — build green; awaiting runtime validation.
