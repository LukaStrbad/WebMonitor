using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;
using Microsoft.OpenApi.Extensions;
using Microsoft.Win32;
using System.Management;

namespace WebMonitor.Native.Disk;

public partial class DiskInfo
{
    /// <summary>
    /// Disk type (HDD, SSD)
    /// </summary>
    public required string? DiskType { get; init; }

    /// <summary>
    /// Connection type (SATA, USB, etc.)
    /// </summary>
    /// <value></value>
    public required string? ConnectionType { get; init; }

    /// <summary>
    /// Name of the disk
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Total disk capacity in bytes
    /// </summary>
    public required ulong TotalSize { get; init; }

    /// <summary>
    /// Value indicating if disk is removable
    /// </summary>
    public required bool IsRemovable { get; init; }

    /// <summary>
    /// Rotation speed in RPM (if applicable)
    /// </summary>
    /// <value></value>
    public uint? RotationalSpeed { get; init; }

    /// <summary>
    /// Physical drive index
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal int DriveIndex { get; init; }

    /// <summary>
    /// Linux name of the disk
    /// </summary>
    internal string LinuxName { get; init; } = "";

    [SupportedOSPlatform("windows8.0")]
    private static IEnumerable<DiskInfo> GetDiskInfosWin()
    {
        var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
        var searcher = new ManagementObjectSearcher(scope, new SelectQuery("MSFT_PhysicalDisk"));

        foreach (var disk in searcher.Get())
        {
            var index = Convert.ToInt32(disk["DeviceId"]);
            var name = disk["FriendlyName"].ToString();
            var diskSize = (ulong)disk["Size"];
            var diskType = GetDiskType((ushort)disk["MediaType"]);
            var connectionType = GetDiskConnectionType((ushort)disk["BusType"]);
            var isRemovable = connectionType is "USB" or "SD" or "FireWire" or "MMC";
            var rotationalSpeed = (uint)disk["SpindleSpeed"];

            yield return new()
            {
                DriveIndex = index,
                DiskType = diskType,
                ConnectionType = connectionType,
                IsRemovable = isRemovable,
                Name = name!,
                TotalSize = diskSize,
                RotationalSpeed = rotationalSpeed is 0 ? null : rotationalSpeed
            };
        }
    }

    /// <summary>
    /// String representation of the disk type from MSFT_PhysicalDisk.MediaType
    /// </summary>
    /// <param name="mediaType">Value of type</param>
    [SupportedOSPlatform("windows8.0")]
    private static string? GetDiskType(ushort mediaType)
        => mediaType switch
        {
            3 => "HDD",
            4 => "SSD",
            5 => "SCM",
            _ => null
        };

    /// <summary>
    /// String representation of the disk connection type from MSFT_PhysicalDisk.BusType
    /// </summary>
    /// <param name="busType">Value of type</param>
    [SupportedOSPlatform("windows8.0")]
    private static string? GetDiskConnectionType(ushort busType)
        => busType switch
        {
            1 => "SCSI",
            2 => "ATAPI",
            3 => "ATA",
            4 => "FireWire",
            5 => "SSA",
            6 => "Fibre Channel",
            7 => "USB",
            8 => "RAID",
            9 => "iSCSI",
            10 => "SAS",
            11 => "SATA",
            12 => "SD",
            13 => "MMC",
            14 => "Virtual",
            15 => "File Backed Virtual",
            16 => "Storage Spaces",
            17 => "NVMe",
            _ => null
        };


    [SupportedOSPlatform("linux")]
    private static IEnumerable<DiskInfo> GetDiskInfosLinux()
    {
        var blockDir = new DirectoryInfo("/sys/class/block");

        foreach (var drive in blockDir.GetDirectories())
        {
            // Only physical drives contain "device" directory
            if (drive.GetDirectories().FirstOrDefault(dir => dir.Name == "device") is null)
                continue;

            var isRotational = File.ReadAllText(Path.Combine(drive.FullName, "queue/rotational")).Contains('1');
            var isRemovable = File.ReadAllText(Path.Combine(drive.FullName, "removable")).Contains('1');
            var name = File.ReadAllText(Path.Combine(drive.FullName, "device/model")).Trim();
            // Size returns a number of blocks, block size is always 512 bytes
            var size = Convert.ToUInt64(File.ReadAllText(Path.Combine(drive.FullName, "size"))) * 512;

            yield return new DiskInfo
            {
                DiskType = isRotational ? "HDD" : "SSD",
                ConnectionType = GetConnectionType(drive.Name),
                IsRemovable = isRemovable,
                Name = name,
                TotalSize = size,
                LinuxName = drive.Name
            };
        }
    }

    [SupportedOSPlatform("linux")]
    private static string? GetConnectionType(string name)
    {
        if (name.StartsWith("fd"))
            return "Floppy";
        if (name.StartsWith("hd"))
            return "IDE";
        if (name.StartsWith("sd"))
            return "SATA";
        if (name.StartsWith("mmcblk"))
            return "MMC";
        if (name.StartsWith("nvme"))
            return "NVMe";
        
        return null;
    }

    /// <summary>
    /// Returns information about physical drives
    /// </summary>
    public static IEnumerable<DiskInfo> GetDiskInfos()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
            return GetDiskInfosWin();

        if (OperatingSystem.IsLinux())
            return GetDiskInfosLinux();

        return Enumerable.Empty<DiskInfo>();
    }
}
