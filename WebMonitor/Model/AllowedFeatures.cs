using System.Text.Json.Serialization;

namespace WebMonitor.Model;

public class AllowedFeatures
{
    /// <summary>
    /// The ID of the allowed features object for the database.
    /// </summary>
    [JsonIgnore]
    public int Id { get; set; }

    public bool CpuUsage { get; set; }
    public bool MemoryUsage { get; set; }
    public bool DiskUsage { get; set; }
    public bool NetworkUsage { get; set; }
    public bool GpuUsage { get; set; }
    public bool RefreshIntervalChange { get; set; }
    public bool NvidiaRefreshSettings { get; set; }
    public bool Processes { get; set; }
    public bool FileBrowser { get; set; }
    public bool FileDownload { get; set; }
    public bool FileUpload { get; set; }
    public bool ExtendedProcessInfo { get; set; }
    public bool ProcessPriorityChange { get; set; }
    public bool ProcessAffinityChange { get; set; }
    public bool Terminal { get; set; }
    public bool BatteryInfo { get; set; }

    public static AllowedFeatures All => new()
    {
        CpuUsage = true,
        MemoryUsage = true,
        DiskUsage = true,
        NetworkUsage = true,
        GpuUsage = true,
        RefreshIntervalChange = true,
        NvidiaRefreshSettings = true,
        Processes = true,
        FileBrowser = true,
        FileDownload = true,
        FileUpload = true,
        ExtendedProcessInfo = true,
        ProcessPriorityChange = true,
        ProcessAffinityChange = true,
        Terminal = true,
        BatteryInfo = true
    };

    public static AllowedFeatures None => new();
}
