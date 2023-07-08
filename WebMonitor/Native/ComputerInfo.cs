using System.Runtime.Versioning;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Memory;

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
    public string OsBuild { get; }

    /// <summary>
    /// CPU info
    /// </summary>
    public CpuInfo? Cpu { get; }

    /// <summary>
    /// Memory info
    /// </summary>
    public MemoryInfo? Memory { get; }

    /// <summary>
    /// List disk infos
    /// </summary>
    public IEnumerable<DiskInfo>? Disks { get; }

    [SupportedOSPlatform("linux")] private readonly Dictionary<string, string>? _osReleaseValues;

    public ComputerInfo(IComputer computer, SupportedFeatures supportedFeatures)
    {
        if (OperatingSystem.IsLinux())
        {
            var file = "/etc/os-release";
            if (!File.Exists(file))
                file = "/etc/gentoo-release";
            if (!File.Exists(file))
                file = "/etc/SuSE-release";

            if (File.Exists(file))
            {
                _osReleaseValues = File
                    .ReadAllLines(file)
                    .Select(line => line.Split('='))
                    .ToDictionary(line => line[0], line => line[1]);
            }
        }

        Hostname = Environment.MachineName;
        CurrentUser = Environment.UserName;
        OsName = GetOsName();
        OsVersion = GetOsVersion();
        OsBuild = GetOsBuild();
        if (supportedFeatures.CpuInfo)
            Cpu = new CpuInfo(computer);
        if (supportedFeatures.MemoryInfo)
            Memory = new MemoryInfo();
        if (supportedFeatures.DiskInfo)
            Disks = DiskInfo.GetDiskInfos();
    }

    private string GetOsName()
    {
        if (OperatingSystem.IsWindows())
        {
            // 22000 is the first build number of Windows 11
            // Older versions of Windows are not supported
            var baseVersion = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)
                ? "Windows 11"
                : "Windows 10";

            var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var edition = versionKey?.GetValue("EditionID")?.ToString();

            return $"{baseVersion} {edition}";
        }

        if (OperatingSystem.IsLinux() && _osReleaseValues?.TryGetValue("NAME", out var value) is true)
        {
            return value.Trim('"');
        }

        return "";
    }

    private string GetOsVersion()
    {
        if (OperatingSystem.IsWindows())
        {
            var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var version = versionKey?.GetValue("DisplayVersion")?.ToString();

            return version ?? "";
        }

        if (OperatingSystem.IsLinux() && _osReleaseValues?.TryGetValue("VERSION", out var value) is true)
        {
            return value.Trim('"');
        }

        return "";
    }

    private static string GetOsBuild()
    {
        if (OperatingSystem.IsWindows())
        {
            var buildNumber = Environment.OSVersion.Version.Build;

            return buildNumber == -1 ? "" : buildNumber.ToString();
        }

        if (OperatingSystem.IsLinux())
        {
            var version = Environment.OSVersion.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}-{version.Revision}";
        }

        return "";
    }
}