using Windows.Win32;

namespace WebMonitorLib.Memory;

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
    /// <remarks>Currently only implemented on Linux</remarks>
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
        else if (OperatingSystem.IsLinux())
        {
            var memInfo = File
                .ReadAllLines("/proc/meminfo")
                .Select(line => line.Split(':'))
                .ToDictionary(split => split[0].Trim(), split => split[1].Trim());

            Total = long.Parse(memInfo["MemTotal"].Split(' ')[0]) * 1024;
            var available = long.Parse(memInfo["MemAvailable"].Split(' ')[0]) * 1024;
            Used = Total - available;
            var swapTotal = long.Parse(memInfo["SwapTotal"].Split(' ')[0]) * 1024;
            var swapFree = long.Parse(memInfo["SwapFree"].Split(' ')[0]) * 1024;
            var usedSwap = (swapTotal - swapFree);
            Commited = Used + usedSwap;
            Cached = long.Parse(memInfo["Cached"].Split(' ')[0]) * 1024;
        }
    }
}