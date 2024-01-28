using LibreHardwareMonitor.Hardware;
using Microsoft.OpenApi.Extensions;
using WebMonitorLib.Utility;

namespace WebMonitorLib.Gpu;

public class LhmGpuUsage : IGpuUsage
{
    private readonly IHardware _gpu;
    private readonly ISensor? _loadSensor;
    private readonly ISensor? _memoryUsedSensor;
    private readonly ISensor? _memoryTotalSensor;
    private readonly ISensor? _coreClockSensor;
    private readonly ISensor? _memoryClockSensor;
    private readonly ISensor? _powerSensor;
    private readonly ISensor? _temperatureSensor;

    internal UpdateVisitor? UpdateVisitor { get; init; }

    public string Manufacturer { get; }
    public string Name { get; }
    public float Utilization => _loadSensor?.Value ?? 0;
    public long MemoryUsed => NumberUtility.MbToB(_memoryUsedSensor?.Value ?? 0f);
    public long MemoryTotal => NumberUtility.MbToB(_memoryTotalSensor?.Value ?? 0f);
    public uint CoreClock => (uint)(_coreClockSensor?.Value ?? 0);
    public uint MemoryClock => (uint)(_memoryClockSensor?.Value ?? 0);
    public float? Power => _powerSensor?.Value;
    public float Temperature => _temperatureSensor?.Value ?? 0;

    public LhmGpuUsage(IHardware gpu)
    {
        _gpu = gpu;
        Name = gpu.Name;

        Manufacturer = gpu.HardwareType switch
        {
            HardwareType.GpuAmd => "AMD",
            HardwareType.GpuIntel => "Intel",
            HardwareType.GpuNvidia => "NVIDIA",
            _ => throw new NotSupportedException($"Unsupported hardware type: {gpu.HardwareType.GetDisplayName()}")
        };

        _loadSensor = Array.Find(gpu.Sensors, s => s.SensorType is SensorType.Load);

        _memoryUsedSensor = Array.Find(gpu.Sensors, s =>
            s.SensorType is SensorType.SmallData
            && s.Name.Contains("memory used", StringComparison.OrdinalIgnoreCase));

        _memoryTotalSensor = Array.Find(gpu.Sensors, s =>
            s.SensorType is SensorType.SmallData
            && s.Name.Contains("memory total", StringComparison.OrdinalIgnoreCase));

        _coreClockSensor = Array.Find(gpu.Sensors, s =>
            s.SensorType is SensorType.Clock
            && s.Name.Contains("clock", StringComparison.OrdinalIgnoreCase));

        _memoryClockSensor = Array.Find(gpu.Sensors, s =>
            s.SensorType is SensorType.Clock
            && s.Name.Contains("memory", StringComparison.OrdinalIgnoreCase));

        _powerSensor = Array.Find(gpu.Sensors, s => s.SensorType is SensorType.Power);
        _temperatureSensor = Array.Find(gpu.Sensors, s => s.SensorType is SensorType.Temperature);
    }

    public void Refresh(int millisSinceRefresh)
    {
        if (UpdateVisitor is null)
            return;

        _gpu.Accept(UpdateVisitor);
    }
}