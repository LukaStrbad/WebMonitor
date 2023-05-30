namespace WebMonitor.Native.Gpu;

public interface IGpuUsage : IRefreshable
{

    /// <summary>
    /// Core clock in MHz
    /// </summary>
    uint CoreClock { get; }

    /// <summary>
    /// Memory clock in MHz
    /// </summary>
    uint MemoryClock { get; }

    /// <summary>
    /// Bytes of total memory
    /// </summary>
    long MemoryTotal { get; }

    /// <summary>
    /// Bytes of used memory
    /// </summary>
    long MemoryUsed { get; }

    /// <summary>
    /// GPU name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Power consumption in Watts
    /// </summary>
    /// <remarks>Some GPUs don't support reading this value</remarks>
    float? Power { get; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    float Temperature { get; }

    /// <summary>
    ///  GPU load as a percentage (0 - 100)
    /// </summary>
    float Utilization { get; }

    /// <summary>
    /// GPU manufacturer
    /// </summary>
    string Manufacturer { get; }

}
