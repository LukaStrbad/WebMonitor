using ManagedCuda.Nvml;
using System.Runtime.InteropServices;
using WebMonitor.Options;

namespace WebMonitor.Native.Gpu;

public class NvidiaGpuUsage : IGpuUsage
{
    private readonly nvmlDevice _gpu;
    private readonly NvidiaRefreshSettings _refreshSettings;
    private int _iteration = 0;

    public string Manufacturer => "NVIDIA";

    public uint CoreClock { get; private set; }

    public uint MemoryClock { get; private set; }

    public long MemoryTotal { get; private set; }

    public long MemoryUsed { get; private set; }

    public string Name { get; }

    public float? Power { get; private set; }

    public float Temperature { get; private set; }

    public float Utilization { get; private set; }

    private NvidiaGpuUsage(nvmlDevice gpu, NvidiaRefreshSettings refreshSettings)
    {
        ThrowIfNotX64();

        _gpu = gpu;
        _refreshSettings = refreshSettings;
        // Get name
        NvmlNativeMethods.nvmlDeviceGetName(gpu, out var name);
        Name = name;
    }

    public void Refresh(int millisSinceRefresh)
    {
        ThrowIfNotX64();

        // NOTE: Refreshing clocks and power sometimes causes high CPU usage for unknown reasons
        // nvidia-smi exhibits the same behavior so there may be no way to fix this
        // Possible workarounds:
        // - Disable clock and power monitoring
        // - Refresh those values less often

        // If disabled, do nothing
        if (_refreshSettings.RefreshSetting == NvidiaRefreshSetting.Disabled)
            return;

        // If longer interval, check if it is time to refresh
        if (_refreshSettings.RefreshSetting == NvidiaRefreshSetting.LongerInterval)
        {
            // Loop back to 0 when we reach the end
            _iteration = (_iteration + 1) % _refreshSettings.NRefreshIntervals;
            if (_iteration != 0)
                return;
        }

        // If enabled refresh everything
        uint val = 0;
        // Refresh memory
        var memory = new nvmlMemory();
        NvmlNativeMethods.nvmlDeviceGetMemoryInfo(_gpu, ref memory);
        MemoryTotal = (long)memory.total;
        MemoryUsed = (long)memory.used;

        // Refresh temperature
        NvmlNativeMethods.nvmlDeviceGetTemperature(_gpu, nvmlTemperatureSensors.Gpu, ref val);
        Temperature = val;

        // Refresh utilization
        var utilization = new nvmlUtilization();
        NvmlNativeMethods.nvmlDeviceGetUtilizationRates(_gpu, ref utilization);
        Utilization = utilization.gpu;

        // If partially disabled, do not refresh clocks and power
        if (_refreshSettings.RefreshSetting == NvidiaRefreshSetting.PartiallyDisabled)
            return;
        // Refresh clocks
        NvmlNativeMethods.nvmlDeviceGetClockInfo(_gpu, nvmlClockType.Graphics, ref val);
        CoreClock = val;
        NvmlNativeMethods.nvmlDeviceGetClockInfo(_gpu, nvmlClockType.Mem, ref val);
        MemoryClock = val;

        // Refresh power
        NvmlNativeMethods.nvmlDeviceGetPowerUsage(_gpu, ref val);
        Power = val / 1000f;
    }

    public static IEnumerable<NvidiaGpuUsage> GetNvidiaGpus(NvidiaRefreshSettings refreshSetting)
    {
        ThrowIfNotX64();

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
            yield return new NvidiaGpuUsage(device, refreshSetting);
        }
    }

    /// <summary>
    /// Method to check if NVML is supported on this system
    /// </summary>
    internal static bool CheckIfSupported()
    {
        try
        {
            var gpus = GetNvidiaGpus(new NvidiaRefreshSettings());
            var first = gpus.First();
            var memory = new nvmlMemory();
            // Only check one value to avoid long refresh delay if the GPU is not running
            NvmlNativeMethods.nvmlDeviceGetMemoryInfo(first._gpu, ref memory);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Throws if OS architecture is not x64
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown if OS architecture is not x64</exception>
    private static void ThrowIfNotX64()
    {
        if (RuntimeInformation.OSArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException("Only x64 is supported");
        }
    }
}
