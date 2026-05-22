# Issue #12 — Persist theme & language globally

## Problem
- `LoginWindow.xaml.cs` ctor (lines 33-39) hardcodes `ThemePalette.Apply(dark: false)` + `ApplicationThemeManager.Apply(Light)` + `ThemeToggle.IsChecked = false` + `LanguageManager.Instance.SetLanguage("fr")`.
- `MainWindow.xaml.cs` ctor (lines 39-45) does the **same** hardcoded init — so any choice made on the login screen is wiped when the shell opens.
- Theme and language change handlers in both windows update in-memory state only; nothing is written to disk, so a restart resets to defaults.
- `App.xaml.cs::OnStartup` only calls `ApiConfig.Initialize()`, no central settings load.

User-visible symptom: pick dark / pick Arabic on login → log in → reset to light/French. Close app → reopen → reset again.

## Design

**One source of truth** = a singleton `AppSettingsService` persisting to JSON, loaded once in `App.OnStartup`, applied centrally before any window appears. Per-window ctors only **sync UI controls** (toggle state, combobox index) to the already-applied settings; handlers `Save()` after applying.

### Settings shape
```json
{ "IsDarkTheme": false, "LanguageCode": "fr" }
```

### Storage
- Path: `%APPDATA%/BombaProMax/settings.json` (`Environment.SpecialFolder.ApplicationData`).
- Serializer: `System.Text.Json`, case-insensitive.
- Missing/corrupt → silent fallback to defaults (`dark=false`, `code="fr"`).
- `Save()` ensures directory exists, overwrites file (single-user desktop, atomic write not required).

### Singleton pattern
Mirrors `LanguageManager.Instance`. Public `Instance`, public mutable `IsDarkTheme` + `LanguageCode`, `Load()` / `Save()`.

### Ctor IsChecked guard
Setting `ThemeToggle.IsChecked = settings.IsDarkTheme` after `InitializeComponent()` fires `Checked`/`Unchecked` handlers — those handlers also `Save()`. To avoid a redundant write during construction, gate handlers with a `_isInitializing` field set true during ctor, false after.

## Files to touch

| File | Change |
|---|---|
| `BombaProMaxWPF/Services/AppSettingsService.cs` | **CREATE** — singleton + Load/Save + JSON persistence. |
| `BombaProMaxWPF/App.xaml.cs` | `OnStartup`: `Load()` then `ThemePalette.Apply` + `ApplicationThemeManager.Apply` + `LanguageManager.SetLanguage` from persisted values. |
| `BombaProMaxWPF/Views/LoginWindow.xaml.cs` | Ctor: replace hardcoded init with `_isInitializing` block syncing UI from settings. Handlers (`ThemeToggle_Checked`, `ThemeToggle_Unchecked`, `LanguageComboBox_SelectionChanged`): write back to `AppSettingsService.Instance` and call `Save()`, guarded by `_isInitializing`. |
| `BombaProMaxWPF/MainWindow.xaml.cs` | Same edits as LoginWindow. |

No XAML changes. No new packages.

## Validation
1. Login screen: toggle to **dark** → log in → shell still dark. ✅
2. Login screen: pick **Arabic** → log in → shell still Arabic, RTL preserved. ✅
3. Toggle dark on shell → close app → reopen → login appears in dark. ✅
4. Pick Arabic on shell → close app → reopen → login appears in Arabic. ✅
5. Delete `settings.json` while app closed → reopen → loads defaults (light/fr) without error. ✅
6. `dotnet build` green.

## Out of scope
- DI container for the settings service (defer; matches LanguageManager singleton).
- Migration from any prior settings format (none exists).
- Settings UI surface (the toggle + combobox already exist).

---
**Reply `GO` to apply.**
