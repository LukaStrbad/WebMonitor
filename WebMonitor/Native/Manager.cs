using System.Diagnostics;
using System.Runtime.Versioning;
using WebMonitor.Model;
using SystemProcess = System.Diagnostics.Process;

namespace WebMonitor.Native;

public class Manager
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

    public ProcessPriorityClass? ChangePriority(int pid, ProcessPriorityClass priority)
    {
        if (!_supportedFeatures.ProcessPriorityChange)
            return null;

        var process = SystemProcess.GetProcessById(pid);
        process.PriorityClass = priority;
        return process.PriorityClass;
    }

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