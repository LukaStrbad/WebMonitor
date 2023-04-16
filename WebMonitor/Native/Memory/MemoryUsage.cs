using Windows.Win32;

namespace WebMonitor.Native.Memory;

public class MemoryUsage : IRefreshable
{
    /// <summary>
    /// Amount of memory used in bytes
    /// </summary>
    public long Used { get; private set; }

    /// <summary>
    /// Amount of physical and virtual memory used in bytes
    /// </summary>
    public long Commited { get; private set; }

    /// <summary>
    /// Amount of cached memory in bytes
    /// </summary>
    public long Cached { get; private set; }

    /// <summary>
    /// Memory capacity in bytes
    /// </summary>
    public long Total { get; private set; }

    public void Refresh(int millisSinceRefresh)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            PInvoke.GlobalMemoryStatus(out var memoryStatus);
            Total = (long)memoryStatus.dwTotalPhys;
            Used = Total - (long)memoryStatus.dwAvailPhys;
            Commited = (long)memoryStatus.dwTotalPageFile - (long)memoryStatus.dwAvailPageFile;
            
        }
    }
}