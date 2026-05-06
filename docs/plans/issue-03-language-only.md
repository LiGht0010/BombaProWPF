# Issue #3 — Language switch flips the whole layout

## What user reported
> "only the language should change not the whole sides of every thing"

When picking Arabic, the entire shell mirrors (sidebar jumps to the right, icons reverse, etc.). The user only wants the **strings** to switch language; the layout stays LTR for both French and Arabic.

## Diagnosis
**Where it lives:** `BombaProMaxWPF/MainWindow.xaml.cs` → `LanguageComboBox_SelectionChanged`.

```csharp
LanguageManager.Instance.SetLanguage(code);

FlowDirection = code == "ar"
    ? System.Windows.FlowDirection.RightToLeft
    : System.Windows.FlowDirection.LeftToRight;
```

The first line already does the job — `LanguageManager` raises `Item[]` PropertyChanged so every `Binding [Key]` updates its text in place. The second statement (the `FlowDirection` assignment on the window) is what mirrors the entire visual tree, because `FlowDirection` cascades to every child by inheritance.

**Why it happens:** the original assumption was Arabic = RTL UI. The user's design choice is LTR for both languages — only the text content is localized.

## Proposed fix (apply only on GO)
- Remove the `FlowDirection = ...` block from `LanguageComboBox_SelectionChanged` so only `LanguageManager.Instance.SetLanguage(code)` runs.
- Leave the window's default `FlowDirection` (LTR) untouched.
- No XAML change required (no explicit `FlowDirection` is set in `MainWindow.xaml`).
- Build, verify switching to العربية now only swaps strings.

## Status
✅ Done — `FlowDirection` flip removed from `LanguageComboBox_SelectionChanged`. Plus localized: sidebar tooltips (`TogglePaneTooltip`, `LogoutTooltip`), CTA (`StartDay`), and the 9 sidebar nav titles via new keys `NavDashboard`/`NavVentes`/`NavAchats`/`NavCaisse`/`NavClients`/`NavInfrastructure`/`NavRessources`/`NavRapports`/`NavParametres` in both `Strings.resx` and `Strings.ar.resx`. `NavItem` now implements `INotifyPropertyChanged` and refreshes `Title` on `LanguageManager.LanguageChanged`. Build green.
