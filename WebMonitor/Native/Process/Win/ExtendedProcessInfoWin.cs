using System.Diagnostics;
using System.Runtime.Versioning;

namespace WebMonitor.Native.Process.Win;

public class ExtendedProcessInfoWin : ExtendedProcessInfo
{
    /// <summary>
    /// Process priority
    /// </summary>
    public ProcessPriorityClass PriorityWin { get; init; }


    [SupportedOSPlatform("windows")]
    public ExtendedProcessInfoWin(int pid) : base(pid)
    {
        Owner = ProcessTracker.GetProcessOwnerWin(pid);
        PriorityWin = Process.PriorityClass;
    }
}