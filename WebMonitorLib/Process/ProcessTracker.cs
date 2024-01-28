using System.Runtime.Versioning;
using System.Security.Principal;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using SystemProcess = System.Diagnostics.Process;

namespace WebMonitorLib.Process;

/// <summary>
/// A class that tracks CPU and disk usage (WIP) of each process
/// </summary>
internal class ProcessTracker : IRefreshable
{
    private readonly Dictionary<int, TimeSpan> _previousCpu;

    public Dictionary<int, ProcessInfo> Processes { get; }

    public ProcessTracker()
    {
        _previousCpu = new Dictionary<int, TimeSpan>(255);
        Processes = new Dictionary<int, ProcessInfo>(255);
    }

    public void Refresh(int millisSinceRefresh)
    {
        Processes.Clear();
        foreach (var process in SystemProcess.GetProcesses())
        {
            try
            {
                if (_previousCpu.TryGetValue(process.Id, out var cpuTime))
                {
                    _previousCpu.Remove(process.Id);
                    _previousCpu.Add(process.Id, process.TotalProcessorTime);

                    var timeDiff = process.TotalProcessorTime - cpuTime;

                    Processes[process.Id] = ProcessToProcessInfo(process, millisSinceRefresh,
                        timeDiff.TotalMilliseconds);
                }
                else
                {
                    _previousCpu.Remove(process.Id);
                    _previousCpu.Add(process.Id, process.TotalProcessorTime);
                    Processes[process.Id] = ProcessToProcessInfo(process, millisSinceRefresh, 0);
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    private static ProcessInfo ProcessToProcessInfo(SystemProcess process, int millisSinceRefresh, double workingMillis)
    {
        string? owner = null;
        if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
            owner = GetProcessOwnerWin(process);
        else if (OperatingSystem.IsLinux())
            owner = GetProcessOwnerLinux(process.Id);

        return new ProcessInfo
        {
            Pid = process.Id,
            Owner = owner,
            Name = process.ProcessName,
            MemoryUsage = process.PrivateMemorySize64,
            DiskUsage = 0,
            CpuUsage = (float)(workingMillis / millisSinceRefresh * 100 / Environment.ProcessorCount)
        };
    }

    [SupportedOSPlatform("windows5.1.2600")]
    internal static string? GetProcessOwnerWin(SystemProcess process)
    {
        var processHandle = IntPtr.Zero;
        try
        {
            unsafe
            {
                var result = PInvoke.OpenProcessToken((HANDLE)process.Handle, TOKEN_ACCESS_MASK.TOKEN_QUERY,
                    (HANDLE*)&processHandle);
                if (result.Value == 0)
                    return null;
            }

            using var wi = new WindowsIdentity(processHandle);
            var user = wi.Name;
            return user.Contains('\\') ? user[(user.IndexOf(@"\", StringComparison.Ordinal) + 1)..] : user;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (processHandle != IntPtr.Zero)
            {
                PInvoke.CloseHandle((HANDLE)processHandle);
            }
        }
    }

    [SupportedOSPlatform("linux")] private static readonly Dictionary<uint, string> IdToUser = new();

    [SupportedOSPlatform("linux")]
    internal static string? GetProcessOwnerLinux(int processId)
    {
        try
        {
            var userId = uint.Parse(File.ReadAllText($"/proc/{processId}/loginuid"));

            if (userId == 0xFFFFFFFF)
                return "root";

            if (!IdToUser.ContainsKey(userId))
                RefreshIdToUser();

            // If the user is still not found, null is returned below
            return IdToUser[userId];
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("linux")]
    internal static void RefreshIdToUser()
    {
        IdToUser.Clear();
        var lines = File.ReadAllLines("/etc/passwd");
        foreach (var line in lines)
        {
            var split = line.Split(':');
            // The user id is the third field
            var userId = uint.Parse(split[2]);
            // The user name is the first field
            var userName = split[0];
            IdToUser.Add(userId, userName);
        }
    }
}