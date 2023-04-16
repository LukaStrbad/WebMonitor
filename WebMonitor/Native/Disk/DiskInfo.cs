using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;
using Microsoft.OpenApi.Extensions;
using Microsoft.Win32;

namespace WebMonitor.Native.Disk;

public partial class DiskInfo
{
    /// <summary>
    /// Disk type
    /// </summary>
    public required string DiskType { get; init; }

    /// <summary>
    /// Name of the disk
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Total disk capacity in bytes
    /// </summary>
    public required long TotalSize { get; init; }

    /// <summary>
    /// Value indicating if disk is removable
    /// </summary>
    public required bool IsRemovable { get; init; }
    
     /// <summary>
    /// Relative path from "LocalMachine" that contains indices of physical drives
    /// </summary>
    private const string DiskEnumPath = @"SYSTEM\CurrentControlSet\Services\disk\Enum";

    /// <summary>
    /// Drive geometry constant
    /// </summary>
    private const int IOCTL_DISK_GET_DRIVE_GEOMETRY = (0x00000007 << 16) | (0 << 14) | (0x0000 << 2) | 0;

    /// <summary>
    /// Physical drive index
    /// </summary>
    [SupportedOSPlatform("windows10.0")]
    internal int DriveIndex { get; init; }

    [SupportedOSPlatform("windows10.0")]
    private static IEnumerable<DiskInfo> GetDiskInfosWin()
    {
        using var subKey = Registry.LocalMachine.OpenSubKey(DiskEnumPath, false);

        if (subKey is null)
            throw new NullReferenceException(nameof(subKey));

        var nameRegex = NameRegex();

        foreach (var key in subKey.GetValueNames())
        {
            if (!int.TryParse(key, out var index)) continue;

            var name = nameRegex.Match(subKey.GetValue(key)?.ToString() ?? "");
            var geometry = GetDriveGeometry(index);
            var diskSize = geometry.Cylinders * geometry.TracksPerCylinder
                                              * geometry.SectorsPerTrack * geometry.BytesPerSector;

            yield return new()
            {
                DriveIndex = index,
                DiskType = geometry.MediaType.ToString(),
                IsRemovable = geometry.MediaType == MEDIA_TYPE.RemovableMedia,
                Name = name.ToString(),
                TotalSize = diskSize
            };
        }
    }

    [SupportedOSPlatform("windows10.0")]
    private static DISK_GEOMETRY GetDriveGeometry(int volumeNum)
    {
        // Handle to the physical drive
        using var handle = PInvoke.CreateFile(
            $@"\\.\PhysicalDrive{volumeNum}",
            0,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            0,
            null
        );

        if (handle.IsInvalid)
            throw new Exception();

        using var geometryPtr = new DisposablePointer<DISK_GEOMETRY>();
        unsafe
        {
            // Call to get DISK_GEOMETRY
            var result = PInvoke.DeviceIoControl(
                handle,
                IOCTL_DISK_GET_DRIVE_GEOMETRY,
                (void*)0,
                0,
                (void*)geometryPtr,
                (uint)geometryPtr.Size,
                (uint*)0,
                (NativeOverlapped*)0
            );

            if (result == 0)
                throw new Exception();
        }

        return geometryPtr[0];
    }

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
            var size = Convert.ToInt64(
                File.ReadAllText(Path.Combine(drive.FullName, "size"))
            );

            yield return new DiskInfo
            {
                DiskType = isRotational ? "HDD" : "SSD",
                IsRemovable = isRemovable,
                Name = name,
                TotalSize = size
            };
        }
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

    [GeneratedRegex("(?<=Prod_).+?(?=\\\\)")]
    private static partial Regex NameRegex();
}