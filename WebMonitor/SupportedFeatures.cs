using System.Diagnostics;
using LibreHardwareMonitor.Hardware;
using WebMonitor.Controllers;
using WebMonitor.Native;
using WebMonitor.Native.Battery;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Memory;
using WebMonitor.Native.Network;
using WebMonitor.Native.Process;
using WebMonitor.Native.Process.Linux;
using WebMonitor.Plugins;

namespace WebMonitor;

/// <summary>
/// A class containing all the features that are supported by the server.
/// </summary>
public class SupportedFeatures
{
    public bool CpuInfo { get; set; }
    public bool MemoryInfo { get; set; }
    public bool DiskInfo { get; set; }
    public bool CpuUsage { get; set; }
    public bool MemoryUsage { get; set; }
    public bool DiskUsage { get; set; }
    public bool NetworkUsage { get; set; }
    public bool NvidiaGpuUsage { get; set; }
    public bool AmdGpuUsage { get; set; }
    public bool IntelGpuUsage { get; set; }
    public bool Processes { get; set; }
    public bool FileBrowser { get; set; }
    public bool FileDownload { get; set; }
    public bool FileUpload { get; set; }
    public bool NvidiaRefreshSettings { get; set; }
    public bool BatteryInfo { get; set; }
    public bool ProcessPriority { get; set; }
    public bool ProcessPriorityChange { get; set; }
    public bool ProcessAffinity { get; set; }
    public bool Terminal { get; set; }

    public static SupportedFeatures Detect(ILogger<SupportedFeatures> logger)
    {
        var supportedFeatures = new SupportedFeatures();
        var updateVisitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsBatteryEnabled = true
        };

        computer.Open();
        computer.Accept(updateVisitor);

        supportedFeatures.CpuInfo = CheckFeature(logger, () => new CpuInfo(computer));
        supportedFeatures.MemoryInfo = CheckFeature(logger, () => new MemoryInfo());
        supportedFeatures.DiskInfo = CheckFeature(logger, () => Native.Disk.DiskInfo.GetDiskInfos());
        supportedFeatures.CpuUsage = CheckFeature(logger, () =>
        {
            var cpuUsage = new CpuUsage();
            cpuUsage.Refresh(1000);
        });
        supportedFeatures.MemoryUsage = CheckFeature(logger, () =>
        {
            var memoryUsage = new MemoryUsage();
            memoryUsage.Refresh(1000);
        });
        supportedFeatures.DiskUsage = CheckFeature(logger, () =>
        {
            var diskUsage = new DiskUsageTracker();
            diskUsage.Refresh(1000);
        });
        supportedFeatures.NetworkUsage = CheckFeature(logger, () =>
        {
            var networkUsage = new NetworkUsageTracker();
            networkUsage.Refresh(1000);
        });
        supportedFeatures.Processes = CheckFeature(logger, () =>
        {
            var processTracker = new ProcessTracker();
            processTracker.Refresh(1000);
        });

        supportedFeatures.FileBrowser = CheckFeature(logger, () =>
        {
            var rootDirs = FileBrowserController.GetRootDirs(logger);
        });
        supportedFeatures.FileDownload = supportedFeatures.FileBrowser;
        supportedFeatures.FileUpload = supportedFeatures.FileBrowser;

        // Check if Nvidia GPU usage is supported
        supportedFeatures.NvidiaGpuUsage = Native.Gpu.NvidiaGpuUsage.CheckIfSupported();
        if (!supportedFeatures.NvidiaGpuUsage)
            logger.LogWarning("NVIDIA GPU usage is not supported");

        if (OperatingSystem.IsLinux())
        {
            // AMD GPU usage is not yet supported on Linux
            supportedFeatures.AmdGpuUsage = false;
            supportedFeatures.IntelGpuUsage = false;
            // This is unnecessary on Linux
            supportedFeatures.NvidiaRefreshSettings = false;
            logger.LogWarning("AMD GPU usage, Intel GPU usage and NVIDIA refresh settings are not supported on Linux");
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(logger, () =>
            {
                var currentProcess = Process.GetCurrentProcess();
                var priority = ExtendedProcessInfoLinux.getpriority(0, currentProcess.Id);
                var result = Manager.setpriority(0, currentProcess.Id, priority);
                if (result == -1)
                    throw new Exception("Error setting priority");
            });
        }
        else if (OperatingSystem.IsWindows())
        {
            supportedFeatures.AmdGpuUsage = true;
            supportedFeatures.IntelGpuUsage = false;
            logger.LogWarning("Intel GPU usage is not supported on Windows");
            supportedFeatures.NvidiaRefreshSettings = supportedFeatures.NvidiaGpuUsage;
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(logger, () =>
            {
                var currentProcess = Process.GetCurrentProcess();
                currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
            });
        }

        supportedFeatures.BatteryInfo = CheckFeature(logger, () =>
        {
            var batteryInfo =
                new BatteryInfo(computer.Hardware.First(hardware => hardware.HardwareType == HardwareType.Battery))
                {
                    UpdateVisitor = updateVisitor
                };
            batteryInfo.Refresh(1000);
        });

        supportedFeatures.ProcessAffinity = OperatingSystem.IsWindows() || OperatingSystem.IsLinux();
        if (!supportedFeatures.ProcessAffinity)
            logger.LogWarning("Process affinity is only supported on Windows and Linux");

        return supportedFeatures;
    }

    private static bool CheckFeature(ILogger logger, Action action)
    {
        try
        {
            var task = Task.Run(action);

            // Wait for the task to complete or timeout after 5 seconds
            return task.Wait(TimeSpan.FromSeconds(5)) && task.IsCompletedSuccessfully;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Feature is not supported");
            return false;
        }
    }

    public void ReevaluateWithPlugins(PluginLoader pluginLoader)
    {
        Terminal = pluginLoader.TerminalPlugin is not null;
    }
}
