# Issue #13 — Infrastructure view + sub-section caching infrastructure

## Decisions locked
- **Q1 = B**: segmented pill bar at top of multi-section views (custom `RadioButton` group, not `TabControl`).
- **Q3 = B+**: two-tier view caching with async data lifecycle.
- **Q5 = 4 tabs**: Jaugeage folded into Réservoirs (handled inside that section, not a top-level pill).

## Goals
1. Land the **reusable plumbing** that every multi-section sidebar view will use:
   - `NeuSegmentedPillStyle` (RadioButton skin) — used by Infrastructure here, Ventes/Caisse/Ressources/Paramètres later.
   - `IAsyncLoadable` interface — contract for any view/VM that needs lazy data load.
   - `MainWindow` view cache (`Dictionary<string, UserControl>`) — instantiate once, swap on nav.
2. Stand up `InfrastructureView` with 4 sub-sections (skeleton placeholders for now — actual data port comes per-section in later issues).
3. Wire the `infrastructure` nav key into `MainWindow.OnNavigationRequested`.

## Caching pattern (formal)

### Tier 1 — Shell view cache
`MainWindow` keeps `Dictionary<string, UserControl> _viewCache`. `OnNavigationRequested` hits cache first; if miss, instantiates + stores. Views are **never disposed** during a session — the user explicitly logged out (which closes MainWindow entirely) is the only teardown path.

### Tier 2 — Async data lifecycle
- New `BombaProMaxWPF/Services/IAsyncLoadable.cs`:
  ```csharp
  public interface IAsyncLoadable
  {
      bool IsLoaded { get; }
      Task EnsureLoadedAsync(CancellationToken ct = default);
      Task RefreshAsync(CancellationToken ct = default);
  }
  ```
- Views that need data implement `IAsyncLoadable` on their VM (or directly), hook `Loaded` → `await EnsureLoadedAsync()`, hook `Unloaded` → cancel via a `CancellationTokenSource`.
- `EnsureLoadedAsync` is idempotent (no-op when `IsLoaded == true`).
- `RefreshAsync` forces reload (sets `IsLoaded=false` then calls `EnsureLoadedAsync`).
- While loading, view shows a busy overlay (skeleton or spinner) bound to `IsBusy`.

### Why this combo
- Chrome built **once per session**.
- Data fetched **once, lazily, on first show** — not in ctor (ctor would block instantiation), not on every nav (defeats the cache).
- Switching back to a previously-visited section is **instant** (cache hit + `IsLoaded` short-circuits load).
- Refresh is explicit and per-section, not a hidden side effect of navigation.
- Cancellation prevents stale results from a slow network landing in a view the user has already left.

## Layout — Infrastructure shell

```
InfrastructureView (UserControl, Background=Transparent)
└── ScrollViewer
    └── StackPanel (vertical)
        ├── Header row: title + subtitle
        ├── Segmented pill bar (4 NeuSegmentedPillStyle RadioButtons, group="InfraTabs")
        │     [ Produits ] [ Réservoirs ] [ Pompes ] [ Services ]
        └── ContentControl x:Name="SectionHost" — swapped on pill check
              ├── ProduitsSection.xaml
              ├── ReservoirsSection.xaml      ← Jaugeage lives inside this UC
              ├── PompesSection.xaml
              └── ServicesSection.xaml
```

Sub-sections start as skeleton placeholders (NeuDashCardStyle card with title + "À venir — port en cours" message). Each one is an `IAsyncLoadable` candidate but ships now without data wiring.

## Files to touch

| File | Change |
|---|---|
| `BombaProMaxWPF/Services/IAsyncLoadable.cs` | **CREATE** — interface. |
| `BombaProMaxWPF/Styles/SegmentedPill.xaml` | **CREATE** — `NeuSegmentedPillStyle` (RadioButton template). |
| `BombaProMaxWPF/App.xaml` | Merge `Styles/SegmentedPill.xaml`. |
| `BombaProMaxWPF/Resources/Strings.resx` | Add 8 keys: `InfraTitle`, `InfraSubtitle`, `InfraProduits`, `InfraReservoirs`, `InfraPompes`, `InfraServices`, `InfraRefresh`, `InfraComingSoon`. |
| `BombaProMaxWPF/Resources/Strings.ar.resx` | Same 8 keys, AR translations. |
| `BombaProMaxWPF/Views/InfrastructurePages/InfrastructureView.xaml(.cs)` | **CREATE** — shell with pill bar + SectionHost; caches sub-section UCs in a dict; wires Loaded/Unloaded async hooks (placeholder for now). |
| `BombaProMaxWPF/Views/InfrastructurePages/Sections/ProduitsSection.xaml(.cs)` | **CREATE** — skeleton card. |
| `BombaProMaxWPF/Views/InfrastructurePages/Sections/ReservoirsSection.xaml(.cs)` | **CREATE** — skeleton card. |
| `BombaProMaxWPF/Views/InfrastructurePages/Sections/PompesSection.xaml(.cs)` | **CREATE** — skeleton card. |
| `BombaProMaxWPF/Views/InfrastructurePages/Sections/ServicesSection.xaml(.cs)` | **CREATE** — skeleton card. |
| `BombaProMaxWPF/MainWindow.xaml.cs` | Add `_viewCache` Dictionary; rewrite `OnNavigationRequested` to use it; add `infrastructure` key. |

## Validation
1. Build green.
2. Click Infrastructure in sidebar → view shows with 4 pills, Produits selected by default, skeleton card visible.
3. Click each pill → SectionHost swaps content, no flash.
4. Navigate away (Dashboard) → back to Infrastructure → same pill still selected (state preserved = cache works).
5. Theme toggle while Infrastructure view shown → pills + cards retint correctly.
6. Language switch FR ↔ AR → pill labels + section title update without layout flip.

## Out of scope (deferred to later issues)
- Actual data ports (Produits CRUD, Réservoirs management, Pompes config, Services catalog, Jaugeage readings inside Réservoirs).
- ViewModel ports from MAUI side.
- The other 4 multi-section sidebar views (Ventes, Caisse, Ressources, Paramètres) — they'll reuse `NeuSegmentedPillStyle` + the cache pattern when their turn comes.
