using System.Diagnostics;
using LibreHardwareMonitor.Hardware;
using WebMonitorLib.Battery;
using WebMonitorLib.Cpu;
using WebMonitorLib.Disk;
using WebMonitorLib.Fs;
using WebMonitorLib.Memory;
using WebMonitorLib.Network;
using WebMonitorLib.Process;
using WebMonitorLib.Process.Linux;

namespace WebMonitorLib;

/// <summary>
/// A class containing all the features that are supported by the server.
/// </summary>
public class SupportedFeatures : ISupportedFeatures
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

    /// <summary>
    /// This will contain any warnings that were generated during the detection process.
    /// </summary>
    public List<(Exception, string)> Warnings { get; } = [];

    public static SupportedFeatures Detect(Action<Exception?, string> onWarning)
    {
        var supportedFeatures = new SupportedFeatures();
        var updateVisitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsBatteryEnabled = true
        };

        // computer.Open();
        // computer.Accept(updateVisitor);

        supportedFeatures.CpuInfo = CheckFeature(onWarning, () => new CpuInfo(computer));
        supportedFeatures.MemoryInfo = CheckFeature(onWarning, () => new MemoryInfo());
        supportedFeatures.DiskInfo = CheckFeature(onWarning, () => Disk.DiskInfo.GetDiskInfos());
        supportedFeatures.CpuUsage = CheckFeature(onWarning, () =>
        {
            var cpuUsage = new CpuUsage();
            cpuUsage.Refresh(1000);
        });
        supportedFeatures.MemoryUsage = CheckFeature(onWarning, () =>
        {
            var memoryUsage = new MemoryUsage();
            memoryUsage.Refresh(1000);
        });
        supportedFeatures.DiskUsage = CheckFeature(onWarning, () =>
        {
            var diskUsage = new DiskUsageTracker();
            diskUsage.Refresh(1000);
        });
        supportedFeatures.NetworkUsage = CheckFeature(onWarning, () =>
        {
            var networkUsage = new NetworkUsageTracker();
            networkUsage.Refresh(1000);
        });
        supportedFeatures.Processes = CheckFeature(onWarning, () =>
        {
            var processTracker = new ProcessTracker();
            processTracker.Refresh(1000);
        });

        supportedFeatures.FileBrowser = CheckFeature(onWarning, () =>
        {
            try
            {
                var rootDirs = FilesystemHelper.GetRootDirs();
            }
            catch (FsException e)
            {
                onWarning(e.InnerException, e.Message);
            }
        });
        supportedFeatures.FileDownload = supportedFeatures.FileBrowser;
        supportedFeatures.FileUpload = supportedFeatures.FileBrowser;

        // Check if Nvidia GPU usage is supported
        supportedFeatures.NvidiaGpuUsage = Gpu.NvidiaGpuUsage.CheckIfSupported();
        if (!supportedFeatures.NvidiaGpuUsage)
            onWarning(null, "NVIDIA GPU usage is not supported");

        if (OperatingSystem.IsLinux())
        {
            // AMD GPU usage is not yet supported on Linux
            supportedFeatures.AmdGpuUsage = false;
            supportedFeatures.IntelGpuUsage = false;
            // This is unnecessary on Linux
            supportedFeatures.NvidiaRefreshSettings = false;
            onWarning(null, "AMD GPU usage, Intel GPU usage and NVIDIA refresh settings are not supported on Linux");
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(onWarning, () =>
            {
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                // Reachability is checked, this warning can be ignored
#pragma warning disable CA1416
                var priority = ExtendedProcessInfoLinux.getpriority(0, currentProcess.Id);
                var result = Manager.setpriority(0, currentProcess.Id, priority);
#pragma warning restore CA1416
                if (result == -1)
                    throw new Exception("Error setting priority");
            });
        }
        else if (OperatingSystem.IsWindows())
        {
            supportedFeatures.AmdGpuUsage = true;
            supportedFeatures.IntelGpuUsage = false;
            onWarning(null, "Intel GPU usage is not supported on Windows");
            supportedFeatures.NvidiaRefreshSettings = supportedFeatures.NvidiaGpuUsage;
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(onWarning, () =>
            {
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
            });
        }

        supportedFeatures.BatteryInfo = CheckFeature(onWarning, () =>
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
            onWarning(null, "Process affinity is only supported on Windows and Linux");

        return supportedFeatures;
    }

    private static bool CheckFeature(Action<Exception?, string> onWarning, Action action)
    {
        try
        {
            var task = Task.Run(action);

            // Wait for the task to complete or timeout after 5 seconds
            return task.Wait(TimeSpan.FromSeconds(5)) && task.IsCompletedSuccessfully;
        }
        catch (Exception e)
        {
            onWarning(e, "Feature is not supported");
            return false;
        }
    }
}
