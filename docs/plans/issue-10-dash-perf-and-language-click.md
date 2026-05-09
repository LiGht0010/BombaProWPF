# Issue #10 — Dashboard scroll perf + Language picker click area

Status: 🔍 Diagnosed (awaiting GO)

## Symptoms

1. Forecourt Overview dashboard scrolls visibly laggy.
2. Language picker only opens when the small chevron is clicked; users want to click "anywhere" on the picker (icon + label + box).

---

## Root cause — scroll lag

`NeuDashCardStyle` (Styles/Neumorphic.Light.xaml, lines 40–75) renders two stacked `Border`s each carrying a `DropShadowEffect` (BlurRadius=20, ShadowDepth=6).
`DashboardView.xaml` uses that style 3× ⇒ 6 shadow-bearing surfaces. Plus shell-side effects (sidebar, badge, toolbar, start-day button, gradient pill). Every `ScrollViewer` offset change re-rasterises all of them ⇒ jank.

### Fix
- Add `CacheMode="BitmapCache"` on the outer `Grid` of `NeuDashCardStyle`'s template. Caches the rasterised twin-shadow composite; only invalidated when card content/size changes (rare on a dashboard).
- Add `RenderOptions.EdgeMode="Aliased"` on the same Grid (cheap; effects don't need sub-pixel edges).

No XAML changes needed in `DashboardView.xaml` itself — the style change covers all 3 cards.

---

## Root cause — language picker click area

`NeuComboBoxStyle` template (Styles/Neumorphic.Light.xaml, lines 392–485):
- Chevron `Path` is `Width=10 Height=6` — visually tiny.
- The whole 140×40 ToggleButton **is** clickable, but users perceive only the chevron as actionable, and the surrounding "🌐 Langue" label/icon isn't.

### Fix
1. Enlarge the chevron: `Width=12 Height=7`, `Margin=0,0,16,0`. (Style change.)
2. In `MainWindow.xaml`, wrap the existing toolbar group `Globe24 + LanguageLabel + LanguageComboBox` in a `Border x:Name="LanguagePickerHost"` with transparent `Background` and a `MouseLeftButtonUp` handler that sets `LanguageComboBox.IsDropDownOpen = true`. Cursor=Hand.
3. Add the corresponding handler `LanguagePickerHost_MouseLeftButtonUp` in `MainWindow.xaml.cs`.

This makes the entire icon+label+box strip a single click target without changing keyboard or selection behaviour.

---

## Files to touch

| File | Change |
|---|---|
| `BombaProMaxWPF/Styles/Neumorphic.Light.xaml` | `NeuDashCardStyle` template Grid: add `CacheMode="BitmapCache"` + `RenderOptions.EdgeMode="Aliased"`. `NeuComboBoxStyle` chevron: 10×6 → 12×7, margin 14→16. |
| `BombaProMaxWPF/MainWindow.xaml` | Wrap Globe+label+ComboBox in clickable `Border`. |
| `BombaProMaxWPF/MainWindow.xaml.cs` | Add `LanguagePickerHost_MouseLeftButtonUp` handler. |

## Validation
- `dotnet build` green.
- Runtime: scroll dashboard — smooth.
- Runtime: click globe icon, label, or box body — dropdown opens.
- Theme swap still clean (no visual regression on cards).

## Out of scope
- No restructuring of `NeuComboBoxStyle` ToggleButton chrome.
- No changes to other ComboBoxes (none currently use `NeuComboBoxStyle` besides the language picker).
- No virtualization changes; 8 pump tiles + 3 tank tiles is well under the threshold.
