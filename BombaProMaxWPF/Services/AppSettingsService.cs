using System;
using System.IO;
using System.Text.Json;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Persistent, app-wide user preferences (theme + language).
/// Singleton mirrored on <see cref="Localization.LanguageManager"/>'s shape so
/// it can be consumed without DI. Backed by a JSON file under
/// %APPDATA%/BombaProMax/settings.json — missing or corrupt files fall back
/// to defaults silently.
/// </summary>
public sealed class AppSettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BombaProMax");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static AppSettingsService Instance { get; } = new();

    private AppSettingsService() { }

    /// <summary>True when the dark Neumorphic palette is active.</summary>
    public bool IsDarkTheme { get; set; }

    /// <summary>BCP-47-ish culture code understood by <see cref="Localization.LanguageManager"/> (e.g. "fr", "ar").</summary>
    public string LanguageCode { get; set; } = "fr";

    /// <summary>
    /// Reads the persisted settings into <see cref="Instance"/>. Safe to call
    /// once at startup; any I/O or deserialization failure leaves defaults intact.
    /// </summary>
    public void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return;
            }

            var json = File.ReadAllText(SettingsPath);
            var dto = JsonSerializer.Deserialize<SettingsDto>(json, JsonOptions);
            if (dto is null)
            {
                return;
            }

            IsDarkTheme = dto.IsDarkTheme;
            if (!string.IsNullOrWhiteSpace(dto.LanguageCode))
            {
                LanguageCode = dto.LanguageCode!;
            }
        }
        catch
        {
            // Corrupt or unreadable settings — fall back to in-memory defaults.
        }
    }

    /// <summary>
    /// Writes the current settings to disk. Errors are swallowed; preferences
    /// are non-critical and the app must keep running if e.g. the profile is read-only.
    /// </summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var dto = new SettingsDto
            {
                IsDarkTheme = IsDarkTheme,
                LanguageCode = LanguageCode,
            };
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Non-fatal: the user's choice still applies for this session.
        }
    }

    private sealed class SettingsDto
    {
        public bool IsDarkTheme { get; set; }
        public string? LanguageCode { get; set; }
    }
}
