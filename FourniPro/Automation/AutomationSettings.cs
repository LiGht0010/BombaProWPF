using System.Text.Json;

namespace FourniPro.Automation;

/// <summary>
/// Persists per-automation enabled/disabled flags.
/// Backed by the same %APPDATA%/FourniPro directory as <see cref="FourniPro.Services.AppSettingsService"/>,
/// stored in a separate <c>automations.json</c> file.
/// </summary>
public sealed class AutomationSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FourniPro",
        "automations.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static AutomationSettings Instance { get; } = new();

    private AutomationSettings() { }

    // Backing store: automationId → enabled flag.
    private Dictionary<string, bool> _flags = [];

    /// <summary>Loads persisted flags from disk. Safe to call at startup; failures leave all automations enabled.</summary>
    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, bool>>(json, JsonOptions);
            if (loaded is not null) _flags = loaded;
        }
        catch
        {
            // Ignore — defaults (enabled) apply.
        }
    }

    /// <summary>Persists the current flags to disk.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_flags, JsonOptions));
        }
        catch
        {
            // Best-effort.
        }
    }

    /// <summary>Returns <c>true</c> if the automation with the given <paramref name="id"/> is enabled (default: <c>true</c>).</summary>
    public bool IsEnabled(string id) =>
        !_flags.TryGetValue(id, out var value) || value;

    /// <summary>Sets the enabled state for the automation with the given <paramref name="id"/> and persists immediately.</summary>
    public void SetEnabled(string id, bool value)
    {
        _flags[id] = value;
        Save();
    }
}
