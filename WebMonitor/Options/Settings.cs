using System.Numerics;
using System.Security;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebMonitor.Model;

namespace WebMonitor.Options;

public sealed class Settings : SettingsBase
{
    private int _refreshInterval = 1000;

    public const string SettingsPath = "settings.json";

    /// <summary>
    /// Time between refreshes (ms)
    /// </summary>
    public int RefreshInterval
    {
        get => _refreshInterval;
        set => UpdateValue(
            ref _refreshInterval,
            Math.Clamp(value, 1000, int.MaxValue),
            ChangedSettings.RefreshInterval);
    }

    // No need for on change event because the value should be checked every refresh anyway
    public NvidiaRefreshSettings NvidiaRefreshSettings { get; internal init; } = new()
    {
        RefreshSetting = NvidiaRefreshSetting.Enabled,
        NRefreshIntervals = 10
    };

    internal Settings()
    {
    }

    private static Settings LoadImpl(bool loadFromFile)
    {
        if (!loadFromFile || !File.Exists(SettingsPath))
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
        catch (Exception e)
            when (e is UnauthorizedAccessException or IOException or SecurityException)
        {
            Console.WriteLine("Error opening settings file.");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error parsing settings file: {e.Message}");
        }
        catch (FormatException e)
        {
            Console.WriteLine($"Invalid value format: {e.Message}");
        }
        catch
        {
            Console.WriteLine("Unknown error loading settings.");
        }
        
        Console.WriteLine("Using default settings.");

        return new Settings();
    }

    public static Settings Load(bool loadFromFile = true)
    {
        var settings = LoadImpl(loadFromFile);
        settings.SettingsChanged += (_, _) => settings.Save();
        settings.NvidiaRefreshSettings.SettingsChanged +=
            (_, changedSettings) => settings.OnSettingsChanged(changedSettings);
        return settings;
    }

    public void Save()
    {
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

    public override bool Equals(object? obj)
    {
        if (obj is Settings settings)
            return RefreshInterval == settings.RefreshInterval &&
                   NvidiaRefreshSettings.RefreshSetting == settings.NvidiaRefreshSettings.RefreshSetting &&
                   NvidiaRefreshSettings.NRefreshIntervals == settings.NvidiaRefreshSettings.NRefreshIntervals;

        return false;
    }
}