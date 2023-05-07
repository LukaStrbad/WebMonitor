namespace WebMonitor.Native.Memory;

/// <summary>
/// Info about a memory stick
/// </summary>
/// <param name="Manufacturer">Manufacturer</param>
/// <param name="PartNumber">Part number</param>
/// <param name="Capacity">Capacity in bytes</param>
public record MemoryStickInfo(
    string? Manufacturer,
    string? PartNumber,
    ulong Capacity
);