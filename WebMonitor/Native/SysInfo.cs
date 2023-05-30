using System.Timers;
using LibreHardwareMonitor.Hardware;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Gpu;
using WebMonitor.Native.Memory;
using WebMonitor.Native.Network;
using WebMonitor.Native.Process;
using System.Runtime.InteropServices;
using Timer = System.Timers.Timer;
using WebMonitor.Model;

namespace WebMonitor.Native;

internal class SysInfo
{
    private readonly Timer _timer;
    private readonly List<IRefreshable> _refreshables;
    private readonly ProcessTracker _processTracker;
    private readonly NetworkUsageTracker _networkUsageTracker;
    private readonly DiskUsageTracker _diskUsageTracker;

    private const int RefreshInterval = 1000;
    public long LastRefresh { get; private set; }

    public RefreshInformation RefreshInfo => new()
    {
        MillisSinceLastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds() - LastRefresh,
        RefreshInterval = RefreshInterval
    };

    public CpuUsage CpuUsage { get; } = new();
    public ComputerInfo ComputerInfo { get; }
    public MemoryUsage MemoryUsage { get; }
    public IEnumerable<ProcessInfo> Processes => _processTracker.Processes.Values;
    public IEnumerable<NetworkUsage> NetworkUsages => _networkUsageTracker.Interfaces;
    public List<DiskUsage> DiskUsages => _diskUsageTracker.DiskUsages;
    public IEnumerable<IGpuUsage> GpuUsages { get; }

    public NvidiaRefreshSettings NvidiaRefreshSettings { get; } = new()
    {
        RefreshSetting = NvidiaRefreshSetting.Enabled,
        NRefreshIntervals = 10
    };

    public SysInfo()
    {
        var updateVisitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };

        computer.Open();
        computer.Accept(updateVisitor);

        ComputerInfo = new ComputerInfo(computer);
        // Disable CPU because it is only used for one-time info
        computer.IsCpuEnabled = false;

        MemoryUsage = new MemoryUsage();

        var gpuUsages = computer.Hardware
            .Where(hardware =>
                hardware.HardwareType is HardwareType.GpuIntel or HardwareType.GpuAmd)
            .Select(gpu => new LhmGpuUsage(gpu)
            {
                UpdateVisitor = updateVisitor,
            })
            .Cast<IGpuUsage>()
            .ToList();

        // NVML is only supported on x64
        if (RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            // GeForce experience overlay causes high CPU usage
            // Using NVML may fix the issue with the added benefit of linux support
            gpuUsages.AddRange(NvidiaGpuUsage.GetNvidiaGpus(NvidiaRefreshSettings));
        }

        GpuUsages = gpuUsages;

        // Refreshables
        _processTracker = new ProcessTracker();
        _networkUsageTracker = new NetworkUsageTracker();
        _diskUsageTracker = new DiskUsageTracker();

        _refreshables = new List<IRefreshable>
        {
            CpuUsage,
            MemoryUsage,
            _processTracker,
            _networkUsageTracker,
            _diskUsageTracker
        };
        _refreshables.AddRange(GpuUsages);

        // Timer that refreshes every second
        _timer = new Timer(RefreshInterval);
        _timer.Elapsed += Refresh;
        _timer.Start();
    }

    private void Refresh(object? sender, ElapsedEventArgs e)
    {
        // Refresh all refreshables in parallel
        Parallel.ForEach(_refreshables, r => r.Refresh(RefreshInterval));
        LastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
