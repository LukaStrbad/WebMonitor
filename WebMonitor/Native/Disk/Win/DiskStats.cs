using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;

namespace WebMonitor.Native.Disk.Win;

/// <summary>
/// Represents information from DISK_PERFORMANCE
/// </summary>
[SupportedOSPlatform("windows10.0")]
internal class DiskStats
{
    public long BytesRead { get; private init; }
    public long BytesWritten { get; private init; }
    public long ReadTime { get; private init; }
    public long WriteTime { get; private init; }
    public long IdleTime { get; private init; }

    private DiskStats()
    {
    }

    public DiskStats(int driveIndex)
    {
        unsafe
        {
            // Format of the partition handle is "\\.\C:"
            var drive = $@"\\.\PhysicalDrive{driveIndex}";
            using var handle = PInvoke.CreateFile(
                drive,
                // For some reason version 0.2.206-beta of CsWin32 doesn't generate enum FILE_ACCESS_FLAGS
                FILE_ACCESS_FLAGS.FILE_READ_ATTRIBUTES,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_ALWAYS,
                0,
                null
            );

            if (handle.IsInvalid)
                throw new Exception("Invalid handle");

            using var diskPerfPtr = new DisposablePointer<DISK_PERFORMANCE>();

            var result = PInvoke.DeviceIoControl(
                handle,
                0x70020u, // DISK_PERFORMANCE control code
                (void*)0,
                0u,
                (void*)diskPerfPtr.Pointer,
                (uint)diskPerfPtr.Size,
                (uint*)0,
                (NativeOverlapped*)0
            );

            if (result == 0)
                throw new Exception($"{nameof(PInvoke.DeviceIoControl)} call failed");

            var diskPerf = diskPerfPtr[0];
            BytesRead = diskPerf.BytesRead;
            BytesWritten = diskPerf.BytesWritten;
            ReadTime = diskPerf.ReadTime;
            WriteTime = diskPerf.WriteTime;
            IdleTime = diskPerf.IdleTime;
        }
    }

    public static DiskStats operator -(DiskStats lhs, DiskStats rhs)
        => new()
        {
            BytesRead = lhs.BytesRead - rhs.BytesRead,
            BytesWritten = lhs.BytesWritten - rhs.BytesWritten,
            ReadTime = lhs.ReadTime - rhs.ReadTime,
            WriteTime = lhs.WriteTime - rhs.WriteTime,
            IdleTime = lhs.IdleTime - rhs.IdleTime
        };
}