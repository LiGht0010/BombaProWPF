# Issue #11 — Glassmorphism experiment on dashboard cards

Status: 🔍 Diagnosed (awaiting GO)

## Context
User wants to evaluate a glassmorphism card aesthetic (CoachPro reference: lavender/mint gradient bg, frosted-white cards, hairline white border, soft drop shadow) on the dashboard only, **without losing the current neumorphic system**.

## Approach — additive, zero risk to existing styles

1. **New `BombaProMaxWPF/Styles/Glass.xaml`** — independent ResourceDictionary holding:
   - `GlassWindowBackgroundBrush` (diagonal LinearGradientBrush, lavender → mint).
   - `GlassFillBrush` (frosted-white #CC light / #33 dark).
   - `GlassBorderBrush` (hairline #55 light / #22 dark).
   - `GlassCardStyle` (ContentControl): single Border, CornerRadius=20, Padding=20, soft 24-blur drop shadow (opacity 0.18), frosted bg, hairline border.
   - `GlassAccentTileStyle`: same chassis with a tinted overlay using `NeuAccentBrush` at ~18% opacity (for small KPI tiles like "$690.2m").
   - **Alias** `DashboardCardStyle` → `GlassCardStyle` (one-line swap to revert).
2. **`App.xaml`** — merge `Glass.xaml` AFTER `Neumorphic.Light.xaml` so it is additive; no key collisions.
3. **`Theme/ThemePalette.cs`** — add `Glass*` color keys and brush bindings to Light + Dark palettes; build the gradient brush at end of `Apply(...)` (mirrors existing `NeuAccentGradientBrush` pattern). Frozen instances replaced on each Apply (same trick as Neu* brushes to defeat freeze).
4. **`Views/DashboardPages/DashboardView.xaml`** — swap the 3 NeuDashCard ContentControls to `{DynamicResource DashboardCardStyle}`. Set root Grid `Background="{DynamicResource GlassWindowBackgroundBrush}"` so the frost reads.

## Easy rollback
Edit one line in `Glass.xaml`:
```xml
<Style x:Key="DashboardCardStyle" TargetType="ContentControl"
       BasedOn="{StaticResource GlassCardStyle}" />   <!-- glass -->
<!-- vs -->
<Style x:Key="DashboardCardStyle" TargetType="ContentControl"
       BasedOn="{StaticResource NeuDashCardStyle}" /> <!-- neumorphic -->
```

## Out of scope
- LoginWindow / MainWindow shell, sidebar, toolbar — stay neumorphic.
- `NeuDashCardStyle`, `NeuCardStyle`, existing palette keys, `Apply` Neu* logic — untouched.
- No new NuGet. Real-time backdrop blur (acrylic) intentionally skipped; the screenshot reads as a static frosted layer, which a translucent fill simulates well.

## Files to touch
| File | Change |
|---|---|
| `BombaProMaxWPF/Styles/Glass.xaml` | NEW — brushes + styles + DashboardCardStyle alias |
| `BombaProMaxWPF/App.xaml` | Merge Glass.xaml after Neumorphic.Light.xaml |
| `BombaProMaxWPF/Theme/ThemePalette.cs` | Add Glass* keys + brush bindings; build gradient brush in Apply |
| `BombaProMaxWPF/Views/DashboardPages/DashboardView.xaml` | Cards → `DashboardCardStyle`; root `Background` → `GlassWindowBackgroundBrush` |

## Validation
- `dotnet build` green.
- Light mode: frosted-white cards on lavender/mint gradient, dark text legible.
- Dark mode: frosted-charcoal cards on muted-violet/teal gradient, light text legible.
- Theme toggle works without flicker; neumorphic shell + sidebar unchanged.
- Revert path tested: flip alias to NeuDashCardStyle, dashboard reverts cleanly.
