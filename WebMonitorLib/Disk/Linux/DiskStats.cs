using System.Runtime.Versioning;

namespace WebMonitorLib.Disk.Linux;

[SupportedOSPlatform("linux")]
internal class DiskStats
{
    public int BlockSize { get; } = 512;
    public long SectorsRead { get; init; }
    public long SectorsWritten { get; init; }
    public long TimeDoingIOs { get; init; }

    private DiskStats() { }

    public DiskStats(string driveName)
    {
        var drive = $"/sys/class/block/{driveName}";
        var stats = File
            .ReadAllText($"{drive}/stat")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var blockSize = File.ReadAllText($"{drive}/queue/hw_sector_size");

        BlockSize = int.Parse(blockSize);

        SectorsRead = long.Parse(stats[2]);
        SectorsWritten = long.Parse(stats[6]);
        TimeDoingIOs = long.Parse(stats[9]);
    }

    public static DiskStats operator -(DiskStats a, DiskStats b)
    {
        return new()
        {
            SectorsRead = a.SectorsRead - b.SectorsRead,
            SectorsWritten = a.SectorsWritten - b.SectorsWritten,
            TimeDoingIOs = a.TimeDoingIOs - b.TimeDoingIOs
        };
    }
}