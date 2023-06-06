using WebMonitor.Options;

namespace WebMonitorTests;

public class SettingsTests
{
    private const string Json = """
    {
      "RefreshInterval": 500,
      "NvidiaRefreshSettings": {
        "RefreshSetting": 1,
        "NRefreshIntervals": 5
      }
    }
    """;

    private readonly Settings _jsonSettings = new()
    {
        RefreshInterval = 500,
        NvidiaRefreshSettings = new NvidiaRefreshSettings
        {
            RefreshSetting = NvidiaRefreshSetting.PartiallyDisabled,
            NRefreshIntervals = 5
        }
    };

    [Fact]
    public void LoadLoadsCorrectData()
    {
        File.WriteAllText(Settings.SettingsPath, Json);
        var settings = Settings.Load();
        Assert.Equal(_jsonSettings, settings);

        settings = Settings.Load(false);
        Assert.NotEqual(_jsonSettings, settings);
    }

    [Fact]
    public void SavedDataSameAsLoaded()
    {
        var settings = Settings.Load(false);
        settings.Save();
        var loadedSettings = Settings.Load();
        Assert.Equal(settings, loadedSettings);
    }

    [Fact]
    public void SettingsChangedRaised()
    {
        var settings = Settings.Load(false);

        var arg = Assert.Raises<SettingsBase.ChangedSettings>(
            a => settings.SettingsChanged += a,
            a => settings.SettingsChanged -= a,
            () => settings.RefreshInterval = 100);
        Assert.Equal(SettingsBase.ChangedSettings.RefreshInterval, arg.Arguments);
        
        arg = Assert.Raises<SettingsBase.ChangedSettings>(
            a => settings.SettingsChanged += a,
            a => settings.SettingsChanged -= a,
            () => settings.NvidiaRefreshSettings.RefreshSetting = NvidiaRefreshSetting.Disabled);
        Assert.Equal(SettingsBase.ChangedSettings.NvidiaRefreshSetting, arg.Arguments);
        
        arg = Assert.Raises<SettingsBase.ChangedSettings>(
            a => settings.SettingsChanged += a,
            a => settings.SettingsChanged -= a,
            () => settings.NvidiaRefreshSettings.NRefreshIntervals = 20);
        Assert.Equal(SettingsBase.ChangedSettings.NvidiaRefreshIntervals, arg.Arguments);
    }

    [Fact]
    public void SettingsChangedNotRaisedForSameValue()
    {
        var settings = Settings.Load(false);
        settings.RefreshInterval = 400;

        var raised = false;
        settings.SettingsChanged += (_, _) => raised = true;
        settings.RefreshInterval = 400;
        
        Assert.False(raised);
    }
}