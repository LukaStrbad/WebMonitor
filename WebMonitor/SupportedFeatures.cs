using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;
using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Mvc;
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

    public static SupportedFeatures Detect()
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

        supportedFeatures.CpuInfo = CheckFeature(() => new CpuInfo(computer));
        supportedFeatures.MemoryInfo = CheckFeature(() => new MemoryInfo());
        supportedFeatures.DiskInfo = CheckFeature(() => Native.Disk.DiskInfo.GetDiskInfos());
        supportedFeatures.CpuUsage = CheckFeature(() =>
        {
            var cpuUsage = new CpuUsage();
            cpuUsage.Refresh(1000);
        });
        supportedFeatures.MemoryUsage = CheckFeature(() =>
        {
            var memoryUsage = new MemoryUsage();
            memoryUsage.Refresh(1000);
        });
        supportedFeatures.DiskUsage = CheckFeature(() =>
        {
            var diskUsage = new DiskUsageTracker();
            diskUsage.Refresh(1000);
        });
        supportedFeatures.NetworkUsage = CheckFeature(() =>
        {
            var networkUsage = new NetworkUsageTracker();
            networkUsage.Refresh(1000);
        });
        supportedFeatures.Processes = CheckFeature(() =>
        {
            var processTracker = new ProcessTracker();
            processTracker.Refresh(1000);
        });

        var fileBrowserController = new FileBrowserController();
        supportedFeatures.FileBrowser = CheckFeature(() =>
        {
            // Check if root directories can be listed
            var result = fileBrowserController.Dir();
            if (result.Result is not JsonResult)
                throw new Exception();
        });
        supportedFeatures.FileDownload = true;
        supportedFeatures.FileUpload = true;

        // Check if Nvidia GPU usage is supported
        supportedFeatures.NvidiaGpuUsage = Native.Gpu.NvidiaGpuUsage.CheckIfSupported();

        if (OperatingSystem.IsLinux())
        {
            // AMD GPU usage is not yet supported on Linux
            supportedFeatures.AmdGpuUsage = false;
            // This is unnecessary on Linux
            supportedFeatures.NvidiaRefreshSettings = false;
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(() =>
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
            supportedFeatures.NvidiaRefreshSettings = supportedFeatures.NvidiaGpuUsage;
            supportedFeatures.ProcessPriority = true;
            supportedFeatures.ProcessPriorityChange = CheckFeature(() =>
            {
                var currentProcess = Process.GetCurrentProcess();
                currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
            });
        }

        // LibreHardwareMonitor does not support Intel GPUs
        supportedFeatures.IntelGpuUsage = false;

        supportedFeatures.BatteryInfo = CheckFeature(() =>
        {
            var batteryInfo =
                new BatteryInfo(computer.Hardware.First(hardware => hardware.HardwareType == HardwareType.Battery))
                {
                    UpdateVisitor = updateVisitor
                };
            batteryInfo.Refresh(1000);
        });

        supportedFeatures.ProcessAffinity = OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

        return supportedFeatures;
    }

    private static bool CheckFeature(Action action)
    {
        try
        {
            var task = Task.Run(action);

            // Wait for the task to complete or timeout after 5 seconds
            return task.Wait(TimeSpan.FromSeconds(5)) && task.IsCompletedSuccessfully;
        }
        catch
        {
            return false;
        }
    }

    public void ReevaluateWithPlugins(PluginLoader pluginLoader)
    {
        Terminal = pluginLoader.TerminalPlugin is not null;
    }
}
