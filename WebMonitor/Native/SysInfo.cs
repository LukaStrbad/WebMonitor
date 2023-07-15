using System.Timers;
using LibreHardwareMonitor.Hardware;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Gpu;
using WebMonitor.Native.Memory;
using WebMonitor.Native.Network;
using WebMonitor.Native.Process;
using WebMonitor.Native.Battery;
using WebMonitor.Options;
using Timer = System.Timers.Timer;

namespace WebMonitor.Native;

internal class SysInfo
{
    private readonly Settings _settings;
    private readonly Timer _timer;
    private readonly Timer _timer2;
    private readonly List<IRefreshable> _refreshables;
    private readonly List<IRefreshable> _longerIntervalRefreshables;
    private readonly ProcessTracker? _processTracker;
    private readonly NetworkUsageTracker? _networkUsageTracker;
    private readonly DiskUsageTracker? _diskUsageTracker;
    private long LastRefresh { get; set; }
    private long LastRefresh2 { get; set; }

    public RefreshInformation RefreshInfo => new()
    {
        MillisSinceLastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds() - LastRefresh,
        MilllisSinceLastRefresh2 = DateTimeOffset.Now.ToUnixTimeMilliseconds() - LastRefresh2,
        RefreshInterval = _settings.RefreshInterval
    };

    public readonly string? Version;
    public CpuUsage? CpuUsage { get; }
    public ComputerInfo ComputerInfo { get; }
    public MemoryUsage? MemoryUsage { get; }
    public IEnumerable<ProcessInfo>? Processes => _processTracker?.Processes.Values;
    public IEnumerable<NetworkUsage>? NetworkUsages => _networkUsageTracker?.Interfaces;
    public List<DiskUsage>? DiskUsages => _diskUsageTracker?.DiskUsages;
    public IEnumerable<IGpuUsage>? GpuUsages { get; }
    public BatteryInfo? BatteryInfo { get; }

    public SysInfo(Settings settings, string? version, SupportedFeatures supportedFeatures)
    {
        _settings = settings;
        Version = version;

        var updateVisitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsBatteryEnabled = true
        };

        computer.Open();
        computer.Accept(updateVisitor);

        var refreshables = new List<IRefreshable>();

        if (supportedFeatures.CpuUsage)
        {
            CpuUsage = new CpuUsage();
            refreshables.Add(CpuUsage);
        }

        ComputerInfo = new ComputerInfo(computer, supportedFeatures);
        // Disable CPU because it is only used for one-time info
        computer.IsCpuEnabled = false;

        if (supportedFeatures.MemoryUsage)
        {
            MemoryUsage = new MemoryUsage();
            refreshables.Add(MemoryUsage);
        }

        var supportedGpuHardware = new List<HardwareType>();
        if (supportedFeatures.AmdGpuUsage)
            supportedGpuHardware.Add(HardwareType.GpuAmd);
        if (supportedFeatures.IntelGpuUsage)
            supportedGpuHardware.Add(HardwareType.GpuIntel);

        var gpuUsages = computer.Hardware
            .Where(hardware => supportedGpuHardware.Contains(hardware.HardwareType))
            .Select(gpu => new LhmGpuUsage(gpu)
            {
                UpdateVisitor = updateVisitor,
            })
            .Cast<IGpuUsage>()
            .ToList();

        if (supportedFeatures.NvidiaGpuUsage)
        {
            gpuUsages.AddRange(NvidiaGpuUsage.GetNvidiaGpus(_settings.NvidiaRefreshSettings));
        }

        // Only add GPU usage if at least one GPU type is supported
        if (supportedFeatures.AmdGpuUsage || supportedFeatures.IntelGpuUsage || supportedFeatures.NvidiaGpuUsage)
        {
            GpuUsages = gpuUsages;
            refreshables.AddRange(GpuUsages);
        }

        
        if (supportedFeatures.Processes)
        {
            _processTracker = new ProcessTracker();
            refreshables.Add(_processTracker);
        }

        if (supportedFeatures.NetworkUsage)
        {
            _networkUsageTracker = new NetworkUsageTracker();
            refreshables.Add(_networkUsageTracker);
        }

        if (supportedFeatures.DiskUsage)
        {
            _diskUsageTracker = new DiskUsageTracker();
            refreshables.Add(_diskUsageTracker);
        }
        
        var longerIntervalRefreshables = new List<IRefreshable>();

        if (supportedFeatures.BatteryInfo)
        {
            BatteryInfo = new BatteryInfo(computer.Hardware.First(h => h.HardwareType == HardwareType.Battery))
            {
                UpdateVisitor = updateVisitor
            };
            longerIntervalRefreshables.Add(BatteryInfo);
        }

        _refreshables = refreshables;
        _longerIntervalRefreshables = longerIntervalRefreshables;

        // Initial refresh
        Refresh(this, null);
        LongerIntervalRefresh(this, null);
        
        // Timer that refreshes every second
        _timer = new Timer(_settings.RefreshInterval);
        _timer.Elapsed += Refresh;
        _timer.Start();
        
        // Second timer that refreshes 5 times slower than _timer
        _timer2 = new Timer(_settings.RefreshInterval * 5);
        _timer2.Elapsed += LongerIntervalRefresh;
        _timer2.Start();

        _settings.SettingsChanged += (_, changedSettings) =>
        {
            if (changedSettings == SettingsBase.ChangedSettings.RefreshInterval)
                return;

            // Change refresh interval
            _timer.Enabled = false;
            _timer.Interval = _settings.RefreshInterval;
            _timer.Enabled = true;
            
            _timer2.Enabled = false;
            _timer2.Interval = _settings.RefreshInterval * 5;
            _timer2.Enabled = true;
        };
    }

    private void Refresh(object? sender, ElapsedEventArgs? e)
    {
        // Refresh all refreshables in parallel
        Parallel.ForEach(_refreshables, r => r.Refresh(_settings.RefreshInterval));
        LastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private void LongerIntervalRefresh(object? sender, ElapsedEventArgs? e)
    {
        Parallel.ForEach(_longerIntervalRefreshables, r => r.Refresh(_settings.RefreshInterval * 5));
        LastRefresh2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}