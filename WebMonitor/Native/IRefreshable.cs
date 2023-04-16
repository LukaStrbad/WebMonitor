using System.Text.Json.Serialization;

namespace WebMonitor.Native;

public interface IRefreshable
{
    /// <summary>
    /// Method used to refresh values
    /// </summary>
    /// <param name="millisSinceRefresh">Time passed since last call to this method</param>
    public void Refresh(int millisSinceRefresh);

    /// <summary>
    /// LibreHardwareMonitor visitor used to update hardware values
    /// </summary>
    [JsonIgnore]
    UpdateVisitor? UpdateVisitor => null;
}
