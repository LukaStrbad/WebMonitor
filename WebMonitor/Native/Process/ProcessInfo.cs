using System.Text.Json.Serialization;

namespace WebMonitor.Native.Process;

public class ProcessInfo
{
    /// <summary>
    /// Process ID
    /// </summary>
    public required int Pid { get; init; }

    /// <summary>
    /// Process name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Process CPU usage in %
    /// </summary>
    /// <value>A value between 0 and 100</value>
    public required float CpuUsage { get; init; }

    /// <summary>
    /// Process memory usage in bytes
    /// </summary>
    public required long MemoryUsage { get; init; }

    /// <summary>
    /// Process disk usage in %
    /// </summary>
    /// <value>A value between 0 and 100</value>
    public required long DiskUsage { get; init; }
}
