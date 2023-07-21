using System.Diagnostics;
using System.Management;
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
        Owner = GetProcessOwner(pid);
        PriorityWin = Process.PriorityClass;
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
}