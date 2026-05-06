# Shell Polish — Recap & Fix Log

Consolidated summary of every shell-polish issue raised after the first WPF sidebar pass, in chronological order, with the **root cause** and **exact fix** that landed for each.
Companion machine-readable copy: `BombaProMaxWPF/shell-polish-log.json`.

Workflow used throughout: **Diagnose → wait for user `GO` → apply minimal fix → build → mark ✅**.

---

## Issue #1 — Remove placeholder pages
**Symptom.** 9 stub `*Page.xaml` files were generated under `BombaProMaxWPF/Views/Pages/`. Wrong scope (out of polish budget) and wrong folder convention (`Views/[Name]Pages/[Name]View.xaml` is the project rule).

**Root cause.** First pass scaffolded Tableau de bord / Ventes / Achats / Caisse / Clients / Infrastructure / Ressources / Rapports / Paramètres pages preemptively.

**Fix applied.**
- Deleted 18 files (xaml + xaml.cs) under `BombaProMaxWPF/Views/Pages/` and removed the folder.
- Dropped `PageType` from `Models/NavItem.cs`.
- Cleaned `ViewModels/ShellViewModel.BuildItems` — removed `typeof(...)` args + `using BombaProMaxWPF.Views.Pages;`.
- `MainWindow.OnNavigationRequested` reduced to a no-op stub with a `TODO` for the future `Views/[Name]Pages/[Name]View.xaml` ports.

---

## Issue #2 — Outer grid frame around content
**Symptom.** A visible decorative frame surrounded the content area.

**Root cause.** Body Grid had `Margin="16,8,16,16"` painted by an outer container.

**Fix applied.**
- `MainWindow.xaml` body Grid `Margin="16,8,16,16"` → `Margin="0"`.
- Pushed the margins onto children:
  - sidebar `Margin="16,8,8,16"`,
  - content grid `Margin="8,8,16,16"`.

---

## Issue #3 — Language switch flips the whole layout
**Symptom.** Picking Arabic mirrored the entire UI (sidebar moves to right, icons reverse).

**Root cause.** `LanguageComboBox_SelectionChanged` in `MainWindow.xaml.cs` set `FlowDirection = RightToLeft` for `ar`. `FlowDirection` cascades by inheritance through every child.

**Fix applied.**
- Removed the `FlowDirection = …` block; only `LanguageManager.Instance.SetLanguage(code)` runs now.
- Localized previously-hard-coded strings: tooltips (`TogglePaneTooltip`, `LogoutTooltip`), CTA (`StartDay`), and the 9 sidebar nav titles via new keys `NavDashboard`, `NavVentes`, `NavAchats`, `NavCaisse`, `NavClients`, `NavInfrastructure`, `NavRessources`, `NavRapports`, `NavParametres` in both `Resources/Strings.resx` and `Resources/Strings.ar.resx`.
- `Models/NavItem` now implements `INotifyPropertyChanged` and re-raises `Title` on `LanguageManager.LanguageChanged`.

---

## Issue #4 — Theme toggle background glitch (redo)
**Symptom (after first failed attempt).** Toggling Dark only paints the window background dark; cards/sidebar/toolbar stay Light. Toggling back to Light leaves the window background stuck Black.

**Root cause.** Two distinct bugs:
1. **`SolidColorBrush` resources auto-freeze** in WPF when consumed by `Style.Setter`. After freezing, the `Color` DP is immutable — swapping `Neu*Color` keys at runtime is silently ignored, so cards never repaint.
2. **`Wpf.Ui.ApplicationThemeManager.Apply` writes `FluentWindow.Background` as a local DP value.** Local values beat `DynamicResource` markup, so our `Background="{DynamicResource NeuBackgroundBrush}"` loses, and the locally-assigned brush sticks across toggles.

**Fix applied.**
- `Theme/ThemePalette.cs` `Apply(bool dark)` now also writes **6 fresh frozen `SolidColorBrush` instances** into `Application.Current.Resources` (`NeuBackgroundBrush`, `NeuAccentBrush`, `NeuAccentHoverBrush`, `NeuTextPrimaryBrush`, `NeuTextSecondaryBrush`, `NeuInputFillBrush`). Replacing the brush instance defeats the freeze.
- After every `ApplicationThemeManager.Apply(...)` call (3 sites in `MainWindow.xaml.cs` + 3 sites in `Views/LoginWindow.xaml.cs`):
```csharp
this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
```
This re-installs the DynamicResource binding and erases Wpf.Ui's local-value write.

---

## Issue #5 — Collapsed sidebar must hide labels
**Symptom.** Collapse toggle didn't hide the brand text, CTA label, nav labels, or footer text/role.

**Fix applied.** `MainWindow.xaml` got `x:Name`s on `PaneToggleButton`, `StartDayIcon`, `NavIcon` (in `DataTemplate`), `UserAvatar`, `LogoutButton`. `MainWindow.xaml.cs.UpdateCollapsedVisuals` was rewritten to:
- Hide `SidebarBrand`, `UserText`, `StartDayLabel`, `LogoutButton`, every `NavLabel`.
- Center `PaneToggleButton`.
- Zero `StartDayIcon.Margin`.
- Zero `UserAvatar.Margin` and center it.
- Zero each `NavIcon.Margin`, center each, and set each nav `RadioButton.HorizontalContentAlignment = Center`.
- All values restored on expand.

---

## Issue #6 — Collapsed sidebar over-clipped (icons invisible)
**Symptom.** First collapsed pass shrank to 64 px → only thin vertical accent strips visible, no icons.

**Root cause — compounding horizontal padding (60 px overhead!):**
| Layer | Eats |
|------|-----|
| Outer `ContentControl Margin="16,8,8,16"` | 24 px |
| `NeuSidebarStyle.Padding="12"` (left + right) | 24 px |
| Inner header/CTA/footer wrapper `Margin="6,…,6,…"` | 12 px |

Plus inner button styles: `NeuStartDayButtonStyle.Padding="16,10"` and `NeuNavItemStyle.Padding="14,10"`.

**Fix applied.**
- `SidebarCollapsedWidth` 64 → **88** px.
- In `UpdateCollapsedVisuals`, when collapsed:
  - `StartDayButton.Padding = 0,10,0,10` (was `16,10` from style; vertical kept for click target).
  - Each nav `RadioButton.Padding = 0,10,0,10` (was `14,10`).
- Both restored on expand.

---

## Issue #7 — Selected nav: indicator butts icon, HoverFill offset
**Symptom.** Collapsed sidebar shows icons but the selected row's indicator (3 px bar) butts the icon with no breathing room, and the rounded selection highlight (HoverFill) is visibly offset to the right of the icon.

**Root cause.** Two hard-coded values in `NeuNavItemStyle` template:
1. `ContentPresenter HorizontalAlignment="Stretch"` overrides our code-behind `HorizontalContentAlignment=Center` → inner DataTemplate Grid (`Auto / *`) keeps icon glued to col 0 (left).
2. `HoverFill Margin="8,0,2,0"` is asymmetric — visual center is 3 px right of the row center.

**Fix applied (style-only, two surgical edits):**
- `HoverFill.Margin`: `8,0,2,0` → `6,0,6,0` (symmetric).
- `ContentPresenter.HorizontalAlignment`: `Stretch` → `{TemplateBinding HorizontalContentAlignment}` (lets our code-behind setter actually take effect).

---

## Issue #8 — Burger placement / focus halo + footer avatar clipped
**Symptom.**
- Burger button has a dashed focus halo and looks clipped/off-center when collapsed.
- Footer pill area shows only a thin blue arc — avatar clipped to a sliver, exactly the same compounding-padding trap as the nav rows.

**Root cause.**
- **Burger:** sits in col 1 (`Auto`) of a 2-col header Grid `* / Auto`. With 88 px sidebar, header has ~28 px usable — less than the button's `Width=36` → button overflows / clips on the left. `HorizontalAlignment=Center` only centers within col 1 (= button width itself, no-op). Plus WPF's default `Button.FocusVisualStyle` (dashed rectangle) was never overridden in `NeuPaneToggleStyle`.
- **Footer pill:** sidebar inner = 40 px, `UserPill.Margin="6,12,6,4"` eats 12, `NeuUserPillStyle.Padding="10,8"` eats 20 → 8 px content area for a 32 px avatar. Inner Grid `Auto/*/Auto` — col 1 (`*`) absorbs leftover space → avatar pulled left.

**Fix applied.**
- `NeuPaneToggleStyle` got `<Setter Property="FocusVisualStyle" Value="{x:Null}" />` → kills the dashed halo.
- `UpdateCollapsedVisuals`:
  - **Burger:** when collapsed, `Grid.SetColumn(PaneToggleButton, 0)` + `Grid.SetColumnSpan(PaneToggleButton, 2)` + `HorizontalAlignment=Center`. Restored to `Column=1`, `ColumnSpan=1`, `HorizontalAlignment=Right` on expand.
  - **Footer pill:** `Padding=0,8,0,8` collapsed, `10,8,10,8` expanded.
  - **Footer avatar:** when collapsed, `Grid.SetColumnSpan(UserAvatar, 3)` + shrink to 24×24 + `Margin=0` + `HorizontalAlignment=Center`. Restored to `ColumnSpan=1`, 32×32, `Margin=0,0,10,0`, `HorizontalAlignment=Left` on expand.

---

## Cross-cutting lessons learned
- **WPF `SolidColorBrush` auto-freezes** when used in style setters. Color-key swaps don't repaint. → Replace the **brush instance** itself.
- **`Wpf.Ui.ApplicationThemeManager.Apply` writes `FluentWindow.Background` as a local DP value.** Use `SetResourceReference(BackgroundProperty, "...")` after every `Apply` call to re-attach the DynamicResource.
- **`HorizontalAlignment="Stretch"` hard-coded on a `ContentPresenter`** silently swallows `HorizontalContentAlignment` setters. Always bind via `{TemplateBinding HorizontalContentAlignment}` in custom templates.
- **Compounding padding** (outer `Margin` → style `Padding` → inner `Margin` → button `Padding`) eats 60 px before any icon. Never just shrink the column — also strip inner padding when collapsed.
- **Inner Grid layouts** with `*` columns trap children at the left edge regardless of `HorizontalAlignment` settings on the child. Use `Grid.ColumnSpan` to span all cols when you need genuine centering.
- **WPF's default `Button.FocusVisualStyle`** (dashed rectangle) is in effect unless explicitly overridden — set `FocusVisualStyle="{x:Null}"` on icon-only buttons in custom styles.
- **Localization rule for this project:** language switch never flips `FlowDirection`; only translates strings via `LanguageManager` resource indexer (must be present on every view).
- **Encoding gotcha:** PowerShell `Get-Content -Raw | -replace | Set-Content` corrupts non-ASCII (e.g. `●` U+25CF in `PasswordChar`). Always use `[System.IO.File]::ReadAllText / WriteAllText` with explicit UTF-8 BOM, or replace the literal with an XML entity (`&#x25CF;`).

---

## Files touched (master list)
- `BombaProMaxWPF/MainWindow.xaml` — body grid margins, sidebar `x:Name`s.
- `BombaProMaxWPF/MainWindow.xaml.cs` — language switch, theme toggle resource references, `UpdateCollapsedVisuals`.
- `BombaProMaxWPF/Views/LoginWindow.xaml.cs` — theme toggle resource references.
- `BombaProMaxWPF/Theme/ThemePalette.cs` — Color + Brush palette swap.
- `BombaProMaxWPF/Models/NavItem.cs` — `INotifyPropertyChanged`, drop `PageType`.
- `BombaProMaxWPF/ViewModels/ShellViewModel.cs` — localized titles, no `typeof(...)` args.
- `BombaProMaxWPF/Styles/Neumorphic.Light.xaml` — `NeuNavItemStyle` template, `NeuPaneToggleStyle` focus visual.
- `BombaProMaxWPF/Resources/Strings.resx` + `Strings.ar.resx` — 12 new keys.
- `BombaProMaxWPF/Views/Pages/` — entire folder removed.
