# Issue #1 — Remove placeholder pages

## What user reported
The 9 `Views/Pages/*Page.xaml` files were created out-of-scope and don't match the MAUI folder convention `Views/[Name]Pages/[Name]View.xaml` (+ companion `*CreatePopup.xaml`, etc.). Sidebar items can point to nothing (or to the main view) for now — real pages will be ported later in the right structure.

## Diagnosis
**Why it happened:** I scaffolded full Page placeholders to make the Frame navigate immediately, instead of stopping at the sidebar shell.

**Where the dependencies live (must be cleaned in order):**
1. `BombaProMaxWPF/Views/Pages/` — 18 files (9 `.xaml` + 9 `.xaml.cs`) to delete.
2. `BombaProMaxWPF/ViewModels/ShellViewModel.cs` — `BuildItems()` references the 9 page types via `typeof(...)` and `using BombaProMaxWPF.Views.Pages;`. Must be reworked to drop the page-type column from `NavItem` (or set it nullable).
3. `BombaProMaxWPF/Models/NavItem.cs` — has a required `Type PageType` parameter; will become optional / removed.
4. `BombaProMaxWPF/MainWindow.xaml.cs` — `OnNavigationRequested` calls `Activator.CreateInstance(item.PageType)` and navigates `ContentFrame`. Will become a no-op (or just clears the frame) until real pages are ported.
5. `BombaProMaxWPF/Services/NavigationService.cs` — currently unused; safe to keep as-is for later.

## Proposed fix (apply only on GO)
- Delete the 9 `*.xaml` + 9 `*.xaml.cs` files under `Views/Pages/`.
- Delete the now-empty `Views/Pages/` folder.
- `NavItem`: remove `PageType` (sidebar stays selectable but doesn't navigate yet).
- `ShellViewModel.BuildItems()`: drop the `typeof(...)` argument; remove `using BombaProMaxWPF.Views.Pages;`.
- `MainWindow.xaml.cs`: change `OnNavigationRequested` to a no-op stub with a `// TODO: route to real view once Views/[Name]Pages/[Name]View.xaml is ported` comment. Keep the Frame in XAML so the layout doesn't shift.
- Build to confirm green.

## Status
✅ Done — 18 files removed, `NavItem.PageType` dropped, `ShellViewModel.BuildItems` cleaned, `MainWindow.OnNavigationRequested` reduced to no-op stub. Build green.
