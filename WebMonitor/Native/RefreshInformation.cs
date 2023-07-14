namespace WebMonitor.Native;

/// <summary>
/// Class containing information about refreshes
/// </summary>
public class RefreshInformation
{
    /// <summary>
    /// Time since last refresh
    /// </summary>
    public long MillisSinceLastRefresh { get; set; }
    
    /// <summary>
    /// Time since last refresh for the second timer
    /// </summary>

    public long MilllisSinceLastRefresh2 { get; set; }
    
    /// <summary>
    /// Interval between refreshes in milliseconds
    /// </summary>
    /// <value></value>
    public long RefreshInterval { get; set; }
}
