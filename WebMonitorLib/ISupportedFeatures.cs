namespace WebMonitorLib;

public interface ISupportedFeatures
{
    bool CpuInfo { get; set; }
    bool MemoryInfo { get; set; }
    bool DiskInfo { get; set; }
    bool CpuUsage { get; set; }
    bool MemoryUsage { get; set; }
    bool DiskUsage { get; set; }
    bool NetworkUsage { get; set; }
    bool NvidiaGpuUsage { get; set; }
    bool AmdGpuUsage { get; set; }
    bool IntelGpuUsage { get; set; }
    bool Processes { get; set; }
    bool FileBrowser { get; set; }
    bool FileDownload { get; set; }
    bool FileUpload { get; set; }
    bool NvidiaRefreshSettings { get; set; }
    bool BatteryInfo { get; set; }
    bool ProcessPriority { get; set; }
    bool ProcessPriorityChange { get; set; }
    bool ProcessAffinity { get; set; }
    bool Terminal { get; set; }
}