# Issue #8 — Burger placement / focus halo + footer avatar clipped

## What user reported
Screenshot of collapsed sidebar shows:
- Burger button has a dashed rectangle (focus halo) and looks off-center / clipped.
- Footer area shows only a thin blue arc — the user avatar is clipped to a sliver, same compounding-padding problem we fixed on nav rows.

## Diagnosis

### Burger (`PaneToggleButton`)
- **Layout:** sits in col 1 (`Auto`) of a 2-col header Grid (`* / Auto`). Even with col 0 (`*`) absorbing leftover space, our 88 px sidebar leaves ~28 px header width — less than the button's `Width=36`. col 1 overflows → button clipped on left.
- **Halo:** WPF's default `Button.FocusVisualStyle` (dashed rect) is in effect because `NeuPaneToggleStyle` doesn't override it. Focus stays on the button after click.

### Footer (`UserPill`)
- Sidebar inner content area at 88 px column = 40 px.
- `UserPill.Margin="6,12,6,4"` eats 12 px → 28 px.
- `NeuUserPillStyle.Padding="10,8"` eats 20 px → **8 px** for the 32 px avatar = clipped to a strip.
- Inner Grid is `Auto / * / Auto` with avatar in col 0. Even after hiding logout (col 2) and zeroing pill padding, col 1 (`*`) still absorbs leftover space → avatar pulled left, `HorizontalAlignment=Center` only centers within col 0 (= avatar width = no-op).

## Fix applied

### 1. `BombaProMaxWPF/Styles/Neumorphic.Light.xaml` — `NeuPaneToggleStyle`
Added one setter:
```xml
<Setter Property="FocusVisualStyle" Value="{x:Null}" />
```
Kills the dashed halo on burger and (when shown) logout buttons.

### 2. `BombaProMaxWPF/MainWindow.xaml.cs` — `UpdateCollapsedVisuals`

**Burger button — span the header row when collapsed:**
- Expanded: `Column=1`, `ColumnSpan=1`, `HorizontalAlignment=Right`.
- Collapsed: `Column=0`, `ColumnSpan=2`, `HorizontalAlignment=Center`.

**Footer pill — strip horizontal padding when collapsed:**
- Expanded: `Padding=10,8,10,8`.
- Collapsed: `Padding=0,8,0,8`.

**Footer avatar — span the entire pill grid + shrink to fit:**
- Expanded: `ColumnSpan=1`, `Width=Height=32`, `Margin=0,0,10,0`, `HorizontalAlignment=Left`.
- Collapsed: `ColumnSpan=3`, `Width=Height=24`, `Margin=0`, `HorizontalAlignment=Center`.

No `MainWindow.xaml` changes. Build green.

## Status
✅ Applied — build green; awaiting runtime validation.
