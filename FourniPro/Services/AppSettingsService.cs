using System.Text.Json;

namespace FourniPro.Services;

/// <summary>
/// Persistent, app-wide user preferences (theme + language).
/// Backed by a JSON file under %APPDATA%/FourniPro/settings.json.
/// </summary>
public sealed class AppSettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FourniPro");

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

    /// <summary>BCP-47 culture code (e.g. "fr", "ar").</summary>
    public string LanguageCode { get; set; } = "fr";

    /// <summary>Reads persisted settings. Safe to call at startup; any failure leaves defaults intact.</summary>
    public void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;

            var json = File.ReadAllText(SettingsPath);
            var dto = JsonSerializer.Deserialize<SettingsDto>(json, JsonOptions);
            if (dto is null) return;

            IsDarkTheme = dto.IsDarkTheme;
            if (!string.IsNullOrWhiteSpace(dto.LanguageCode))
                LanguageCode = dto.LanguageCode;
        }
        catch
        {
            // Fall back to defaults on any I/O or deserialization failure.
        }
    }

    /// <summary>Persists the current settings to disk.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(new SettingsDto
            {
                IsDarkTheme = IsDarkTheme,
                LanguageCode = LanguageCode,
            }, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Best-effort; ignore failures silently.
        }
    }

    private sealed class SettingsDto
    {
        public bool IsDarkTheme { get; set; }
        public string LanguageCode { get; set; } = "fr";
    }
}
