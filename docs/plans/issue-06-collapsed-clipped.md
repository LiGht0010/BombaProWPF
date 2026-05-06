# Issue #6 — Collapsed sidebar over-clipped (icons invisible)

## What user reported
> Screenshot: collapsed sidebar reduced to a thin vertical bar. Only two short blue accent strips visible (the CTA button background and the selected nav row background). No icons rendered.

## Diagnosis
The 64 px collapsed column from Issue #5 was eaten by compounding horizontal padding layers:

| Layer                                                    | Eats   |
|----------------------------------------------------------|--------|
| Outer `ContentControl Margin="16,8,8,16"`                | 24 px  |
| `NeuSidebarStyle.Padding="12"` (left + right)            | 24 px  |
| Inner header/CTA/footer wrapper `Margin="6,…,6,…"`       | 12 px  |
| **Total fixed overhead before any button**               | **60 px** |

Available content width on a 64 px column = **4 px**. That's why only thin vertical strips were visible: the CTA `Border` and the selected nav `RadioButton`'s indicator paint at ~4 px wide, and the actual `SymbolIcon` glyphs (≥16 px) get clipped to nothing.

Even with a wider column, the inner button styles add more horizontal padding:
- `NeuStartDayButtonStyle.Padding = "16,10"` → 32 px swallowed by the CTA.
- `NeuNavItemStyle.Padding = "14,10"` + a 3 px left indicator → ~31 px swallowed by every nav row.

## Fix applied
**`BombaProMaxWPF/MainWindow.xaml.cs`:**

1. `SidebarCollapsedWidth` 64 → **88** px. Gives 88 − 60 = 28 px for icon content.
2. In `UpdateCollapsedVisuals`, when collapsed:
   - `StartDayButton.Padding = new Thickness(0,10,0,10)` (was `16,10` from style; vertical padding kept for click target). Restored to `16,10,16,10` when expanded.
   - For each nav `RadioButton` in group `ShellNav`: `Padding = new Thickness(0,10,0,10)` collapsed, restored to `14,10,14,10` when expanded.

No XAML or style changes. Build green.

## Status
✅ Applied — build green; awaiting runtime validation.
