# Issue #9 — Neon-Navy dark palette + Forecourt Overview demo dashboard

> Inspired by the user-supplied screenshot (deep navy + cyan→violet gradients, ring charts, pump tiles, market-pricing card, spatial-view hero).
> Companion of the shell-polish series; tracked from `docs/plans/shell-polish.md` row 9.

Status: 🔍 Diagnosed — awaiting `GO`.

---

## 1. What the user asked for
1. Re-color the **dark theme** to match the screenshot's neon-navy + cyan/violet aesthetic. **Light theme stays as-is.**
2. Build a **demo dashboard** that visually clones the screenshot:
   - Title `Forecourt Overview` + station subtitle (📍 Station #442 - North Terminal)
   - Two-column row:
     - **Live Tank Inventory** card with 3 ring charts (Unleaded 75 %, Diesel 50 %, Premium 15 %) + capacity + low-stock alert
     - **Market Pricing** card with 3 fuel-type rows + cyan→violet `Update All Displays` CTA
   - **Active Pump Monitor** strip: 8 pump tiles (status dot + label + IN USE / IDLE / CHECK badge)
   - **Spatial View** hero card (dark gradient placeholder for the satellite tile)
3. Wire the new view to the shell so clicking **Tableau de bord** in the sidebar shows it.

Light/dark toggle must continue to work cleanly (Issue #4 invariants must hold).

---

## 2. Constraints / project rules carried over

| Rule | Source | Application here |
|------|--------|------------------|
| Folder convention `Views/[Name]Pages/[Name]View.xaml` | shell-polish issue #1 | New view lives at `Views/DashboardPages/DashboardView.xaml`. Sub-controls at `Views/DashboardPages/Controls/*.xaml`. |
| Every visible string goes through `LanguageManager` indexer | shell-polish issue #3 | New `Dash*` keys in `Strings.resx` + `Strings.ar.resx`. |
| Theme swap = **brush instance replacement** + `SetResourceReference` after `ApplicationThemeManager.Apply` | shell-polish issue #4 | Any new brush we add must follow the same pattern (registered on both Light + Dark palettes; built fresh and frozen in `ThemePalette.Apply`). |
| Custom `ControlTemplate` `ContentPresenter` must use `{TemplateBinding HorizontalContentAlignment}` | shell-polish issue #7 | Applies to new templates we author. |
| No external NuGet packages without need | global | Rings done with `Path` + `ArcSegment`; gradients with `LinearGradientBrush`; no charting library. |

---

## 3. New "Neon Navy" dark palette

### 3.1 Existing 8 dark colors — replaced

| Key | Old (muted gray) | **New (neon navy)** | Use |
|-----|------------------|---------------------|-----|
| `NeuBackgroundColor` | `#2E3033` | `#0A0F1F` | window/page base |
| `NeuLightShadowColor` | `#4E5358` | `#1B2440` | top-left subtle highlight |
| `NeuDarkShadowColor` | `#0F1012` | `#000000` | bottom-right shadow |
| `NeuAccentColor` | `#6BA5D3` | `#21D4FD` | cyan primary accent |
| `NeuAccentHoverColor` | `#85B8E0` | `#39E0FF` | cyan hover |
| `NeuTextPrimaryColor` | `#E8ECF3` | `#E6EDF3` | titles & headings |
| `NeuTextSecondaryColor` | `#9AA5BA` | `#8997B0` | muted labels |
| `NeuInputFillColor` | `#26282B` | `#11192E` | card surface |

### 3.2 New keys (added to **both** Light and Dark palettes — non-breaking for light theme)

| Key | Light value | Dark value | Use |
|-----|-------------|-----------|-----|
| `NeuAccentSecondaryColor` | `#7B5BD3` | `#B721FF` | violet companion accent |
| `NeuWarnColor` | `#D8A23A` | `#F5C24A` | CHECK / warning badge |
| `NeuDangerColor` | `#D86070` | `#FF7A85` | low-stock / 15 % ring |
| `NeuAccentGradientStart` | `#5AA0D0` | `#21D4FD` | gradient stop A |
| `NeuAccentGradientEnd` | `#7B5BD3` | `#B721FF` | gradient stop B |

Brushes (`NeuAccentSecondaryBrush`, `NeuWarnBrush`, `NeuDangerBrush`) follow the same fresh-frozen-instance rule from issue #4. The gradient is built in code as a `LinearGradientBrush` from start→end and frozen on each `Apply` call (key `NeuAccentGradientBrush`). Light theme gets a tasteful equivalent so any control referencing the gradient still resolves cleanly.

### 3.3 `ThemePalette.cs` changes
- Extend `Light` and `Dark` color dictionaries with the 5 new keys.
- Extend `BrushBindings` with `NeuAccentSecondaryBrush`, `NeuWarnBrush`, `NeuDangerBrush`.
- After the brush loop, build & freeze a `LinearGradientBrush` (start `NeuAccentGradientStart`, end `NeuAccentGradientEnd`, `StartPoint=0,0.5`, `EndPoint=1,0.5`) and write it to `Application.Current.Resources["NeuAccentGradientBrush"]`.

### 3.4 `Styles/Neumorphic.Light.xaml` changes
Declare placeholder resources for the 5 new color keys + 3 new brushes + the gradient brush so `DynamicResource` always resolves at parse time (the runtime swap then overwrites them). Light defaults from the table above.

---

## 4. Dashboard composition

### 4.1 Files added

```
BombaProMaxWPF/
  Views/DashboardPages/
    DashboardView.xaml                      UserControl, page surface
    DashboardView.xaml.cs
    Controls/
      RingProgress.xaml                     Donut chart (Path + ArcSegment)
      RingProgress.xaml.cs                  DPs: Percent, Label, RingBrush, IsLow
      PumpStatusTile.xaml                   Bordered card: title + dot + status badge
      PumpStatusTile.xaml.cs                DPs: PumpName, Status (enum: InUse/Idle/Check)
      FuelPriceRow.xaml                     Border row: label + price + delta caption
      FuelPriceRow.xaml.cs                  DPs: Label, Price, Delta, DeltaTone (enum)
  ViewModels/
    DashboardViewModel.cs                   ObservableObject; Tanks[3], Pumps[8], Prices[3]
  Models/
    TankModel.cs                            Name, FillPercent, Liters, CapacityLiters, IsLow
    PumpModel.cs                            Name, Status (enum)
    FuelPriceModel.cs                       Label, Price, Delta, DeltaTone (enum)
```

### 4.2 Layout (`DashboardView.xaml`)

Outer `Grid`, 4 rows (Auto / Auto / Auto / *) inside a `ScrollViewer`:

1. **Hero title block** (Auto): `Forecourt Overview` (28-32 pt SemiBold) + 📍 station subtitle in `NeuTextSecondaryBrush`. Localized via `Dash.Title` / `Dash.Subtitle`.
2. **Inventory + Pricing row** (Auto): two-column Grid `*,Auto` (Live Tank Inventory stretches, Market Pricing fixed ~320 px).
   - Live Tank Inventory: card (rounded `Border` + neumorphic shadow). Header row: `Live Tank Inventory` left, `SYNCING LIVE` pill right. Body: `UniformGrid Rows=1 Columns=3` of `RingProgress` controls bound to `vm.Tanks`.
   - Market Pricing: card; ItemsControl over `vm.Prices` rendering `FuelPriceRow`; bottom CTA `Button` filled with `NeuAccentGradientBrush`, label `Dash.UpdateDisplays`.
3. **Active Pump Monitor** (Auto): card; `ItemsControl` with `UniformGrid Rows=1 Columns=8` rendering `PumpStatusTile` for each `vm.Pumps[i]`.
4. **Spatial View hero** (* — fills remaining height): card with a `LinearGradientBrush` background `#0E1428 → #050810` plus subtle cyan dots; bottom-left text block `Spatial View` + sub `Real-time occupancy tracking enabled`. Image left out for now.

Card surface = `Border CornerRadius=16 Background=NeuInputFillBrush BorderBrush=NeuAccentBrush Opacity 0.15 BorderThickness=1`.

### 4.3 `RingProgress` control

- Composition: `Grid` containing
  - background ring: `Path` with two `ArcSegment`s describing a full circle, stroke = `NeuTextSecondaryBrush` at low opacity, thickness 8.
  - foreground arc: `Path` with `ArcSegment` whose `Point` is computed from `Percent`, stroke = `RingBrush` (default `NeuAccentGradientBrush`), thickness 8, `StrokeStartLineCap/EndLineCap=Round`.
  - Center stack: percentage label (e.g. `75 %`, 22 pt SemiBold) + small caps title (`UNLEADED`).
- DPs:
  - `Percent` (double 0–100; recomputes the arc end point on change).
  - `Label` (string).
  - `RingBrush` (Brush; default `DynamicResource NeuAccentGradientBrush`).
  - `IsLow` (bool; when true override `RingBrush` with `NeuDangerBrush` and tint label).
- Below the ring (in the host grid, not inside the control): liter count `12,450 L` and capacity caption `Cap: 20,000 L` or `Low Stock Alert` if `IsLow`.

### 4.4 `PumpStatusTile`
- Vertical stack inside a rounded `Border` (~96×104 px). Top: PUMP NN small caps, center: state dot (8 px) tinted by Status, bottom: badge text (`IN USE` / `IDLE` / `CHECK`).
- Status enum drives both dot color and text tone:
  - `InUse` → cyan (`NeuAccentBrush`)
  - `Idle` → muted (`NeuTextSecondaryBrush`)
  - `Check` → warn (`NeuWarnBrush`)

### 4.5 `FuelPriceRow`
- `Border` with subtle background. Two-column grid: left column = label (`UNLEADED`) small caps + price big bold; right column = delta caption (`+0.02 Market` / `Stable` / `-0.05 Target`) tinted by `DeltaTone` (positive/cyan, neutral/muted, negative/danger).

### 4.6 `DashboardViewModel.cs`

Static demo data (no service calls; matches the screenshot):

```csharp
Tanks = new[]
{
    new TankModel("UNLEADED", 75, 12_450, 20_000, false),
    new TankModel("DIESEL",   50, 10_000, 20_000, false),
    new TankModel("PREMIUM",  15,  3_000, 20_000, true),
};

Pumps = new[]
{
    new PumpModel("PUMP 01", PumpStatus.InUse),
    new PumpModel("PUMP 02", PumpStatus.Idle),
    new PumpModel("PUMP 03", PumpStatus.InUse),
    new PumpModel("PUMP 04", PumpStatus.Check),
    new PumpModel("PUMP 05", PumpStatus.Idle),
    new PumpModel("PUMP 06", PumpStatus.InUse),
    new PumpModel("PUMP 07", PumpStatus.Idle),
    new PumpModel("PUMP 08", PumpStatus.Idle),
};

Prices = new[]
{
    new FuelPriceModel("UNLEADED", 1.45m, "+0.02 Market", DeltaTone.Up),
    new FuelPriceModel("DIESEL",   1.62m, "Stable",        DeltaTone.Neutral),
    new FuelPriceModel("PREMIUM",  1.88m, "-0.05 Target",  DeltaTone.Down),
};
```

---

## 5. Shell wire-up

`BombaProMaxWPF/MainWindow.xaml.cs`:
- `OnNavigationRequested(NavItem item)` currently no-op. Replace with:
  - if `item.Key == "dashboard"` → `ContentFrame.Content = new DashboardView();` (lazily; can cache in a private field if perf becomes an issue).
  - else `ContentFrame.Content = null;` (placeholder until other pages get ported in their own issues).
- Default selection of `Tableau de bord` is already wired in `OnLoaded`, so the dashboard appears on launch.

No `MainWindow.xaml` changes required.

---

## 6. Localization keys (Resources/Strings.resx + Strings.ar.resx)

| Key | FR | AR |
|-----|----|----|
| `DashTitle` | Vue d'ensemble du parc | نظرة عامة على المحطة |
| `DashSubtitle` | Station #442 — Terminal Nord | المحطة رقم 442 — الطرف الشمالي |
| `DashLiveTankInventory` | Inventaire des cuves en direct | المخزون الحي للخزانات |
| `DashSyncingLive` | SYNCHRO LIVE | مزامنة حية |
| `DashMarketPricing` | Tarification marché | تسعير السوق |
| `DashUpdateDisplays` | Mettre à jour les afficheurs | تحديث جميع الشاشات |
| `DashActivePumps` | Surveillance des pompes actives | مراقبة المضخات النشطة |
| `DashSpatialView` | Vue spatiale | العرض المكاني |
| `DashSpatialSub` | Suivi d'occupation en temps réel activé | تتبع الإشغال في الوقت الحقيقي مفعّل |
| `DashUnleaded` | SANS PLOMB | بدون رصاص |
| `DashDiesel` | DIESEL | ديزل |
| `DashPremium` | PREMIUM | بريميوم |
| `DashInUse` | EN USAGE | قيد الاستخدام |
| `DashIdle` | INACTIF | متوقف |
| `DashCheck` | À VÉRIFIER | يتطلب فحص |
| `DashLowStockAlert` | Alerte stock bas | تنبيه مخزون منخفض |
| `DashCap` | Cap.: {0} L | السعة: {0} لتر |
| `DashStable` | Stable | مستقر |
| `DashMarketDelta` | {0} Marché | {0} السوق |
| `DashTargetDelta` | {0} Cible | {0} الهدف |

---

## 7. Execution order (when `GO` lands)

1. Extend `ThemePalette.cs` (color dicts + new brush bindings + gradient builder).
2. Add the 5 new color keys + 3 brush keys + gradient placeholder to `Styles/Neumorphic.Light.xaml`.
3. Create `Models/{TankModel,PumpModel,FuelPriceModel}.cs` and `ViewModels/DashboardViewModel.cs`.
4. Create the 3 sub-controls (`RingProgress`, `PumpStatusTile`, `FuelPriceRow`).
5. Create `DashboardView.xaml` + code-behind.
6. Wire `OnNavigationRequested` in `MainWindow.xaml.cs`.
7. Append the new keys to `Resources/Strings.resx` + `Strings.ar.resx`.
8. Build, fix any errors.
9. User runtime-validates against the screenshot.
10. On success → mark Issue #9 ✅ in `shell-polish.md`, append to `shell-polish-recap.md` and `BombaProMaxWPF/shell-polish-log.json`.

---

## 8. Out of scope (next round)

- Real backend wiring for tank/pump/price data.
- Animated ring fills (`DoubleAnimation` on the ArcSegment angle).
- Spatial view satellite imagery (we ship a gradient placeholder).
- Dashboard responsiveness below 1024 px (project `MinWidth=1024`).

---

## Status
✅ Applied — build green; awaiting user runtime validation against the screenshot.

### Implementation deltas vs the original plan
- New models live under `Models/Forecourt/` (namespace `BombaProMaxWPF.Models.Forecourt`) to avoid colliding with the legacy `Models/Dashboard/` namespace inherited from the MAUI port.
- The new VM is `ViewModels/ForecourtDashboardViewModel.cs` (renamed from `DashboardViewModel` for the same reason — a legacy `DashboardViewModel.cs` from the MAUI port already occupies that file slot).
- `RingProgress` selects its stroke through XAML `DataTrigger`s on `IsLow` / `RingBrush == null` rather than a multi-value converter, which keeps `DynamicResource` resolution working without a code-side converter.
- `PumpStatusTile` and `FuelPriceRow` swap their tint via XAML `DataTrigger`s on the `Status` / `DeltaTone` enum.
- `MainWindow.OnNavigationRequested` now routes `item.Key == "dashboard"` → `new DashboardView()`; all other keys still null-out the frame until their own issue ports them.
