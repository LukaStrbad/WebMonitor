namespace WebMonitor.Model;

/// <summary>
/// Settings for how Nvidia GPU monitoring should be handled
/// </summary>
public enum NvidiaRefreshSetting
{
    /// <summary>Everything is enabled</summary>
    Enabled,
    /// <summary>Clock and power are disabled</summary>
    PartiallyDisabled,
    /// <summary>Everything is disabled</summary>
    Disabled,
    /// <summary>Everything is enabled but refreshes occur less often</summary>
    LongerInterval
}

/// <summary>
/// Interface for settings for how Nvidia GPU monitoring should be handled
/// </summary>
public class NvidiaRefreshSettings
{
    /// <summary>
    /// Refresh setting
    /// </summary>
    public NvidiaRefreshSetting RefreshSetting { get; internal set; }

    /// <summary>
    /// Number of refresh intervals to wait between refreshes
    /// </summary>
    public int NRefreshIntervals { get; internal set; }
}
