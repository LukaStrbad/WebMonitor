using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WebMonitorLib.Process.Linux;

public partial class ExtendedProcessInfoLinux : ExtendedProcessInfo
{
    /// <summary>
    /// Process priority
    /// </summary>
    public int PriorityLinux { get; init; }

    [SupportedOSPlatform("linux")]
    public ExtendedProcessInfoLinux(int pid) : base(pid)
    {
        Owner = ProcessTracker.GetProcessOwnerLinux(pid);
        PriorityLinux = getpriority(0, pid);
    }

    [LibraryImport("libc.so.6", EntryPoint = "getpriority"), SupportedOSPlatform("linux")]
    internal static partial int getpriority(int which, int who);
}