using System.Runtime.Versioning;
using WebMonitorLib.Disk.Win;

namespace WebMonitorLib.Disk;

public class DiskUsage : IRefreshable
{
    /// <summary>
    /// Name of the disk
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Current read speed in bytes
    /// </summary>
    public long ReadSpeed { get; set; }

    /// <summary>
    /// Current read speed in bytes
    /// </summary>
    public long WriteSpeed { get; set; }

    /// <summary>
    /// Current disk utilization in %
    /// </summary>
    /// <value>A value between 0 and 100</value>
    public float Utilization { get; set; }

    [SupportedOSPlatform("windows10.0")] private DiskStats _oldDiskStatsWin = default!;

    [SupportedOSPlatform("linux")] private Linux.DiskStats _oldDistStatsLinux = default!;

    private readonly DiskInfo _diskInfo;

    public DiskUsage(DiskInfo diskInfo)
    {
        Name = diskInfo.Name;

        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            _oldDiskStatsWin = new DiskStats(diskInfo.DriveIndex);
        }
        else if (OperatingSystem.IsLinux())
        {
            _oldDistStatsLinux = new Linux.DiskStats(diskInfo.LinuxName);
        }

        _diskInfo = diskInfo;
    }

    public void Refresh(int millisSinceRefresh)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            var newDiskStats = new DiskStats(_diskInfo.DriveIndex);
            var diskStats = newDiskStats - _oldDiskStatsWin;

            var rw = diskStats.ReadTime + diskStats.WriteTime;
            var utilization = (float)rw / (diskStats.IdleTime + rw) * 100f;

            Utilization = float.Clamp(utilization, 0, 100);
            ReadSpeed = diskStats.BytesRead;
            WriteSpeed = diskStats.BytesWritten;

            _oldDiskStatsWin = newDiskStats;
        }
        else if (OperatingSystem.IsLinux())
        {
            var newDiskStats = new Linux.DiskStats(_diskInfo.LinuxName);
            var diskStats = newDiskStats - _oldDistStatsLinux;

            ReadSpeed = diskStats.SectorsRead * newDiskStats.BlockSize;
            WriteSpeed = diskStats.SectorsWritten * newDiskStats.BlockSize;
            Utilization = float.Clamp((float)diskStats.TimeDoingIOs / millisSinceRefresh * 100f, 0, 100);

            _oldDistStatsLinux = newDiskStats;
        }
    }
}