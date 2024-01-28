using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WebMonitorLib.Model;
using ExtendedProcessInfoLinux = WebMonitorLib.Process.Linux.ExtendedProcessInfoLinux;
using SystemProcess = System.Diagnostics.Process;

namespace WebMonitorLib;

public partial class Manager
{
    private readonly SupportedFeatures _supportedFeatures;

    public Manager(SupportedFeatures supportedFeatures)
    {
        _supportedFeatures = supportedFeatures;
    }

    public static string KillProcess(int pid)
    {
        var process = SystemProcess.GetProcessById(pid);
        var name = process.ProcessName;
        process.Kill();
        return name;
    }

    [SupportedOSPlatform("windows")]
    public ProcessPriorityClass? ChangePriorityWin(int pid, ProcessPriorityClass priority)
    {
        if (!_supportedFeatures.ProcessPriorityChange)
            return null;

        var process = SystemProcess.GetProcessById(pid);
        process.PriorityClass = priority;
        return process.PriorityClass;
    }

    [SupportedOSPlatform("linux")]
    public int? ChangePriorityLinux(int pid, int priority)
    {
        if (!_supportedFeatures.ProcessPriorityChange)
            return null;
        
        if (priority is < -20 or > 19)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between -20 and 19");
        
        var result = setpriority(0, pid, priority);
        if (result == -1)
            throw new Exception("Error setting priority");
        
        return ExtendedProcessInfoLinux.getpriority(0, pid);
    }
    
    [LibraryImport("libc.so.6", EntryPoint = "setpriority", SetLastError = true), SupportedOSPlatform("linux")]
    internal static partial int setpriority(int which, int who, int prio);

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    public ulong ChangeAffinity(int pid, List<ChangeAffinityRequest.ThreadInfo> threadInfos)
    {
        if (!_supportedFeatures.ProcessAffinity)
            return 0;
        
        if (threadInfos.Any(t => t.ThreadIndex < 0 || t.ThreadIndex >= Environment.ProcessorCount))
            throw new ArgumentOutOfRangeException(nameof(threadInfos),
                "Thread index must be between 0 and the number of logical processors - 1");

        var process = SystemProcess.GetProcessById(pid);

        foreach (var threadInfo in threadInfos)
        {
            if (threadInfo.On)
            {
                process.ProcessorAffinity |= (nint)(1UL << threadInfo.ThreadIndex);
            }
            else
            {
                ulong bitMask = 1;
                for (var i = 0; i < Environment.ProcessorCount; i++)
                {
                    bitMask *= 2;
                }

                bitMask--;
                bitMask &= ~(1UL << threadInfo.ThreadIndex);

                process.ProcessorAffinity &= (nint)bitMask;
            }
        }

        return (ulong)process.ProcessorAffinity;
    }
}