using System.Text.Json.Serialization;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;

namespace WebMonitor.Native;

public class ComputerInfo
{
    /// <summary>
    /// Hostname of the computer
    /// </summary>
    public string Hostname { get; }

    /// <summary>
    /// Currently logged in user
    /// </summary>
    public string CurrentUser { get; }

    /// <summary>
    /// Name of the OS
    /// </summary>
    public string OsName { get; }

    /// <summary>
    /// OS version
    /// </summary>
    public string OsVersion { get; }
    
    /// <summary>
    /// OS build number
    /// </summary>
    public string? OsBuild { get; }

    /// <summary>
    /// CPU info
    /// </summary>
    public CpuInfo Cpu { get; }

    /// <summary>
    /// List disk infos
    /// </summary>
    public IEnumerable<DiskInfo> Disks { get; }

    public ComputerInfo(Computer computer)
    {
        Hostname = Environment.MachineName;
        CurrentUser = Environment.UserName;
        OsName = GetOsName();
        OsVersion = GetOsVersion();
        OsBuild = GetOsBuild();
        Cpu = new CpuInfo(computer);
        Disks = DiskInfo.GetDiskInfos();
    }
    
    private static string GetOsName()
    {
        if (OperatingSystem.IsWindows())
        {
            // 22000 is the first build number of Windows 11
            // Older versions of Windows are not supported
            var baseVersion = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)
                ? "Windows 11" : "Windows 10";

            var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var edition = versionKey?.GetValue("EditionID")?.ToString();

            return $"{baseVersion} {edition}";
        }

        return "";
    }

    private static string GetOsVersion()
    {
        if (OperatingSystem.IsWindows())
        {
            var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var version = versionKey?.GetValue("DisplayVersion")?.ToString();

            return version ?? "";
        }

        return "";
    }

    private static string? GetOsBuild()
    {
        if (OperatingSystem.IsWindows())
        {
            var buildNumber = Environment.OSVersion.Version.Build;

            return buildNumber == -1 ? null : buildNumber.ToString();
        }

        return null;
    }
}
