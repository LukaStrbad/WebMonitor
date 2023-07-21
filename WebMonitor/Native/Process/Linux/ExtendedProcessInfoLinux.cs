using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WebMonitor.Native.Process.Linux;

public class ExtendedProcessInfoLinux : ExtendedProcessInfo
{
    /// <summary>
    /// Process priority
    /// </summary>
    public int PriorityLinux { get; init; }

    private static Dictionary<int, string> IdToUser = new();

    [SupportedOSPlatform("linux")]
    public ExtendedProcessInfoLinux(int pid) : base(pid)
    {
        Owner = GetProcessOwner(pid);
        PriorityLinux = getpriority(0, pid);
    }

    [SupportedOSPlatform("linux")]
    private static string? GetProcessOwner(int processId)
    {
        try
        {
            var userId = int.Parse(File.ReadAllText($"/proc/{processId}/loginuid"));

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
    private static void RefreshIdToUser()
    {
        IdToUser.Clear();
        var lines = File.ReadAllLines("/etc/passwd");
        foreach (var line in lines)
        {
            var split = line.Split(':');
            // The user id is the third field
            var userId = int.Parse(split[2]);
            // The user name is the first field
            var userName = split[0];
            IdToUser.Add(userId, userName);
        }
    }

    [DllImport("libc.so.6", EntryPoint = "getpriority"), SupportedOSPlatform("linux")]
    private static extern int getpriority(int which, int who);
}