using System.Diagnostics;
using System.Runtime.Versioning;

namespace WebMonitorLib.Process.Win;

public class ExtendedProcessInfoWin : ExtendedProcessInfo
{
    /// <summary>
    /// Process priority
    /// </summary>
    public ProcessPriorityClass PriorityWin { get; init; }


    [SupportedOSPlatform("windows5.1.2600")]
    public ExtendedProcessInfoWin(int pid) : base(pid)
    {
        Owner = ProcessTracker.GetProcessOwnerWin(Process);
        PriorityWin = Process.PriorityClass;
    }
}