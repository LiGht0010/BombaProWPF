# Issue #5 — Collapsed sidebar must hide labels, keep only icons

## What user reported
> "when i minimize the sidebar only the icons stays even for the footer and the journee… like the labels disappears only the icons lasts"

When the sidebar is collapsed: header label, CTA label, nav labels, and user-pill text/role should disappear. Only the icons (logo, play, each nav icon, user avatar) should stay, centered, fully visible — not clipped.

## Diagnosis
**Where it lives:**
- `BombaProMaxWPF/MainWindow.xaml.cs` → `UpdateCollapsedVisuals` (currently only narrows the column + collapses 4 labels) and `SidebarCollapsedWidth` (76 px).
- `BombaProMaxWPF/MainWindow.xaml`:
  - sidebar header (lines ~54–88) — brand stack + collapse-toggle button
  - sticky CTA (lines ~91–101) — `Play24` icon has `Margin="0,0,8,0"` and label
  - nav `DataTemplate` (lines ~109–131) — icon `Margin="0,0,12,0"` + label column
  - footer `UserPill` (lines ~137–180) — avatar + text + logout, 3-column grid

**Why icons end up off-center / clipped today:**
1. Collapsed column is 76 px wide while inner icons keep label-aware margins (`Margin="0,0,12,0"`, `Margin="0,0,8,0"`) → icons drift left.
2. `RadioButton` `NeuNavItemStyle` has `HorizontalContentAlignment="Stretch"` and the inner Grid still has `<ColumnDefinition Width="*" />` for the (now-invisible) label, so the icon stays in column 0 (left), not centered.
3. The footer pill keeps its 3-column avatar/text/logout layout. Collapsing `UserText` leaves the logout button still occupying column 2 → the avatar isn't centered and may clip on a 76 px column.
4. CTA button still stretches and the play icon keeps its right-margin gap reserved for the now-hidden label → icon drifts left.

## Proposed fix (apply only on GO)
Code-behind toggle approach (no style overhaul).

**XAML (`MainWindow.xaml`)**
- Name a few elements so code-behind can flip their alignment/margin on collapse:
  - `x:Name="PaneToggleButton"` on the header collapse `<Button>` (line ~82).
  - `x:Name="StartDayIcon"` on the CTA `<ui:SymbolIcon Symbol="Play24"…>`.
  - `x:Name="UserAvatar"` on the avatar `<Border>` in the footer pill.
  - `x:Name="LogoutButton"` on the footer logout `<Button>`.
- In the nav `DataTemplate`, give each row's icon `x:Name="NavIcon"` and the inner Grid its own `x:Name` is not feasible in a template; instead drive the icon margin via a `<Setter>` keyed on a sibling property. **Simpler:** in code-behind iterate `FindVisualChildren<SymbolIcon>` filtered by `Name == "NavIcon"` and toggle `Margin`.

**Code-behind (`MainWindow.xaml.cs` → `UpdateCollapsedVisuals`)**
- Drop `SidebarCollapsedWidth` from 76 → 64.
- When **collapsed**:
  - Hide labels (already does this).
  - `PaneToggleButton.HorizontalAlignment = Center`.
  - `StartDayIcon.Margin = new Thickness(0)`.
  - `LogoutButton.Visibility = Collapsed`.
  - `UserAvatar.HorizontalAlignment = Center` and zero its `Margin.Right`.
  - For every nav `SymbolIcon` named `NavIcon`: `Margin = new Thickness(0)`, parent `RadioButton.HorizontalContentAlignment = Center`.
- When **expanded**: restore each value (cache the originals or hardcode them — we know the originals from XAML).

This is ~30 lines of code in `UpdateCollapsedVisuals` plus 4 `x:Name` additions in XAML.

## Status
✅ Applied — build green; awaiting runtime validation.
