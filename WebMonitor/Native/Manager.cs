using System.Diagnostics;
using System.Runtime.Versioning;
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
    public ulong ChangeAffinity(int pid, int threadNumber, bool on)
    {
        if (!_supportedFeatures.ProcessAffinity)
            return 0;

        if (threadNumber < 0 || threadNumber >= Environment.ProcessorCount)
            throw new ArgumentOutOfRangeException(nameof(threadNumber),
                "Thread number must be between 0 and the number of logical processors - 1");

        var process = SystemProcess.GetProcessById(pid);
        if (on)
        {
            process.ProcessorAffinity |= (nint)(1UL << threadNumber);
        }
        else
        {
            ulong bitMask = 1;
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                bitMask *= 2;
            }

            bitMask--;
            bitMask &= ~(1UL << threadNumber);

            process.ProcessorAffinity &= (nint)bitMask;
        }

        return (ulong)process.ProcessorAffinity;
    }
}