using ManagedCuda.Nvml;

namespace WebMonitor.Native.Gpu;

public class NvidiaGpuUsage : IGpuUsage
{
    private readonly nvmlDevice _gpu;

    public uint CoreClock { get; private set; }

    public uint MemoryClock { get; private set; }

    public long MemoryTotal { get; private set; }

    public long MemoryUsed { get; private set; }

    public string Name { get; }

    public float? Power { get; private set; }

    public float Temperature { get; private set; }

    public float Utilization { get; private set; }

    private NvidiaGpuUsage(nvmlDevice gpu)
    {
        _gpu = gpu;
        // Get name
        NvmlNativeMethods.nvmlDeviceGetName(gpu, out var name);
        Name = name;
    }

    public void Refresh(int millisSinceRefresh)
    {
        // NOTE: Refreshing clocks and power sometimes causes high CPU usage when NVIDIA overlay is enabled
        // nvidia-smi exhibits the same behavior so there may be no way to fix this
        // Possible workarounds:
        // - Disable NVIDIA overlay
        // - Disable clock and power monitoring
        // - Refresh those values less often

        uint val = 0;

        // Refresh clocks
        NvmlNativeMethods.nvmlDeviceGetClockInfo(_gpu, nvmlClockType.Graphics, ref val);
        CoreClock = val;
        NvmlNativeMethods.nvmlDeviceGetClockInfo(_gpu, nvmlClockType.Mem, ref val);
        MemoryClock = val;

        // Refresh memory
        var memory = new nvmlMemory();
        NvmlNativeMethods.nvmlDeviceGetMemoryInfo(_gpu, ref memory);
        MemoryTotal = (long)memory.total;
        MemoryUsed = (long)memory.used;

        // Refresh power
        NvmlNativeMethods.nvmlDeviceGetPowerUsage(_gpu, ref val);
        Power = val / 1000f;

        // Refresh temperature
        NvmlNativeMethods.nvmlDeviceGetTemperature(_gpu, nvmlTemperatureSensors.Gpu, ref val);
        Temperature = val;

        // Refresh utilization
        var utilization = new nvmlUtilization();
        NvmlNativeMethods.nvmlDeviceGetUtilizationRates(_gpu, ref utilization);
        Utilization = utilization.gpu;
    }

    public static IEnumerable<NvidiaGpuUsage> GetNvidiaGpus()
    {
        // Initialize NVML
        NvmlNativeMethods.nvmlInit();

        // Get device count
        uint deviceCount = 0;
        NvmlNativeMethods.nvmlDeviceGetCount(ref deviceCount);

        // Get devices
        for (uint i = 0; i < deviceCount; i++)
        {
            var device = new nvmlDevice();
            NvmlNativeMethods.nvmlDeviceGetHandleByIndex(i, ref device);
            yield return new NvidiaGpuUsage(device);
        }
    }
}