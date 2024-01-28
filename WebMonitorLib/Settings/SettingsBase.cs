namespace WebMonitorLib.Settings;

/// <summary>
/// Base class for all settings
/// </summary>
public abstract class SettingsBase
{
    /// <summary>
    /// Event that is triggered when some settings change
    /// </summary>
    internal event EventHandler<ChangedSettings>? SettingsChanged;

    /// <summary>
    /// Milliseconds since Unix epoch when settings were last updated
    /// </summary>
    /// <remarks>Time is in UTC</remarks>
    public long LastUpdateTime { get; set; }

    /// <summary>
    /// Method to invoke <see cref="SettingsChanged"/> event
    /// </summary>
    /// <param name="changedSetting">Flags of changed settings</param>
    protected void OnSettingsChanged(ChangedSettings changedSetting)
    {
        SettingsChanged?.Invoke(this, changedSetting);
        LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
