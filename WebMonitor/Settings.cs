using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebMonitor.Model;

namespace WebMonitor;

public sealed class Settings : SettingsBase
{
    private int _refreshInterval = 1000;

    private const string SettingsPath = "settings.json";

    /// <summary>
    /// Time between refreshes (ms)
    /// </summary>
    public int RefreshInterval
    {
        get => _refreshInterval;
        set => UpdateValue(ref _refreshInterval, value, ChangedSettings.RefreshInterval);
    }

    // No need for on change event because the value should be checked every refresh anyway
    public NvidiaRefreshSettings NvidiaRefreshSettings { get; private init; } = new()
    {
        RefreshSetting = NvidiaRefreshSetting.Enabled,
        NRefreshIntervals = 10
    };

    private static Settings LoadImpl(bool loadFromFile)
    {
        if (!loadFromFile)
            return new Settings();

        if (!File.Exists(SettingsPath))
            return new Settings();

        try
        {
            // TODO: Research why regular deserialization doesn't work
            var json = File.ReadAllText(SettingsPath);
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(json);
            if (jsonObject is null)
                return new Settings();
            var nvidiaRefreshSettings = jsonObject[nameof(NvidiaRefreshSettings)];
            return new Settings
            {
                RefreshInterval = jsonObject[nameof(RefreshInterval)]?.GetValue<int>() ?? 1000,
                NvidiaRefreshSettings = new NvidiaRefreshSettings
                {
                    RefreshSetting =
                        (NvidiaRefreshSetting)(nvidiaRefreshSettings?["RefreshSetting"]?.GetValue<int>() ?? 0),
                    NRefreshIntervals = nvidiaRefreshSettings?["NRefreshIntervals"]?.GetValue<int>() ?? 10
                }
            };
        }
        catch
        {
            return new Settings();
        }
    }

    public static Settings Load(bool loadFromFile = true)
    {
        var settings = LoadImpl(loadFromFile);
        settings.SettingsChanged += _ => settings.Save();
        settings.NvidiaRefreshSettings.SettingsChanged +=
            changedSettings => settings.OnSettingsChanged(changedSettings);
        return settings;
    }

    public void Save()
    {
        Console.WriteLine("Saving settings");
        try
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsPath, json);
        }
        catch (IOException)
        {
            Console.WriteLine("Exception");
            Thread.Sleep(100);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsPath, json);
        }
    }
}

/// <summary>
/// Base class for all settings
/// </summary>
public abstract class SettingsBase
{
    /// <summary>
    /// Event that is triggered when some settings change
    /// </summary>
    internal event Action<ChangedSettings>? SettingsChanged;
    
    /// <summary>
    /// Method to invoke <see cref="SettingsChanged"/> event
    /// </summary>
    /// <param name="changedSetting">Flags of changed settings</param>
    protected void OnSettingsChanged(ChangedSettings changedSetting)
    {
        SettingsChanged?.Invoke(changedSetting);
    }
    
    /// <summary>
    /// Method that invokes <see cref="OnSettingsChanged"/> if new value is different from the current value
    /// </summary>
    /// <param name="field">Current field</param>
    /// <param name="value">New value</param>
    /// <param name="changedSetting">Flags of settings that should be changed</param>
    /// <typeparam name="T">Type of field and value</typeparam>
    /// <remarks><see cref="OnSettingsChanged"/> won't be invoked if field and value are the same</remarks>
    internal void UpdateValue<T>(ref T field, T value, ChangedSettings changedSetting)
    {
        if (field?.Equals(value) == true)
            return;

        field = value;
        OnSettingsChanged(changedSetting);
    }
    
    /// <summary>
    /// Enum representing changed settings
    /// </summary>
    [Flags]
    public enum ChangedSettings
    {
        RefreshInterval = 0b_0000_0001,
        NvidiaRefreshSetting = 0b_0000_0010,
        NvidiaRefreshIntervals = 0b_0000_0100
    }
}