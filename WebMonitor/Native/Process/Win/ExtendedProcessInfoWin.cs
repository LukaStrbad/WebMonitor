using System.Management;
using System.Runtime.Versioning;

using SystemProcess = System.Diagnostics.Process;

namespace WebMonitor.Native.Process.Win;

public class ExtendedProcessInfoWin : ExtendedProcessInfo
{
    /// <summary>
    /// Process priority
    /// </summary>
    public ProcessPriority PriorityWin { get; init; }


    [SupportedOSPlatform("windows")]
    public ExtendedProcessInfoWin(int pid) : base(pid)
    {
        Owner = GetProcessOwner(pid);
        PriorityWin = GetPriorityFromNumber(Process.BasePriority);
    }

    [SupportedOSPlatform("windows")]
    private static string? GetProcessOwner(int processId)
    {
        var query = $"Select * From Win32_Process Where ProcessID = {processId}";
        var searcher = new ManagementObjectSearcher(query);
        var processList = searcher.Get();

        foreach (var obj in processList)
        {
            if (obj is not ManagementObject managementObject) continue;

            object[] argList = { string.Empty, string.Empty };
            var returnVal = Convert.ToInt32(managementObject.InvokeMethod("GetOwner", argList));
            if (returnVal == 0)
                return (string)argList[0];
        }

        return null;
    }

    /// <summary>
    /// Returns the process priority from base priority number
    /// </summary>
    /// <param name="value">Base priority number from <see cref="System.Diagnostics.Process.BasePriority"/></param>
    /// <returns><see cref="ProcessPriority"/></returns>
    [SupportedOSPlatform("windows")]
    private static ProcessPriority GetPriorityFromNumber(int value)
        => value switch
        {
            4 => ProcessPriority.Low,
            6 => ProcessPriority.BelowNormal,
            8 => ProcessPriority.Normal,
            10 => ProcessPriority.AboveNormal,
            13 => ProcessPriority.High,
            24 => ProcessPriority.Realtime,
            _ => ProcessPriority.Normal
        };
}