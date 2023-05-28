using System.Runtime.Versioning;

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
    ulong Capacity)
{
    [SupportedOSPlatform("linux")]
    public static MemoryStickInfo FromDictionary(Dictionary<string, string> memStick)
    {
        var manufacturer = memStick.GetValueOrDefault("Manufacturer");
        var partNumber = memStick.GetValueOrDefault("Part Number");
        var capacity = ulong.Parse(memStick.GetValueOrDefault("Size", "0").Split(' ')[0]) * 1024 * 1024;

        return new MemoryStickInfo(manufacturer, partNumber, capacity);
    }
}