using System.Runtime.InteropServices;
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
using WebMonitor.Options;

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

    public SupportedFeatures()
    {
        var updateVisitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsBatteryEnabled = true
        };

        computer.Open();
        computer.Accept(updateVisitor);

        CpuInfo = CheckFeature(() => new CpuInfo(computer));
        MemoryInfo = CheckFeature(() => new MemoryInfo());
        DiskInfo = CheckFeature(() => Native.Disk.DiskInfo.GetDiskInfos());
        CpuUsage = CheckFeature(() =>
        {
            var cpuUsage = new CpuUsage();
            cpuUsage.Refresh(1000);
        });
        MemoryUsage = CheckFeature(() =>
        {
            var memoryUsage = new MemoryUsage();
            memoryUsage.Refresh(1000);
        });
        DiskUsage = CheckFeature(() =>
        {
            var diskUsage = new DiskUsageTracker();
            diskUsage.Refresh(1000);
        });
        NetworkUsage = CheckFeature(() =>
        {
            var networkUsage = new NetworkUsageTracker();
            networkUsage.Refresh(1000);
        });
        Processes = CheckFeature(() =>
        {
            var processTracker = new ProcessTracker();
            processTracker.Refresh(1000);
        });

        var fileBrowserController = new FileBrowserController();
        FileBrowser = CheckFeature(() =>
        {
            // Check if root directories can be listed
            var result = fileBrowserController.Dir();
            if (result.Result is not JsonResult)
                throw new Exception();
        });
        FileDownload = true;
        FileUpload = true;

        // NVML is only supported on x64
        if (RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            NvidiaGpuUsage = CheckFeature(() =>
            {
                var nvidiaGpuUsage = Native.Gpu.NvidiaGpuUsage.GetNvidiaGpus(new NvidiaRefreshSettings
                {
                    RefreshSetting = NvidiaRefreshSetting.Enabled,
                    NRefreshIntervals = 10
                }).FirstOrDefault();
                nvidiaGpuUsage?.Refresh(1000);
            });
        }
        else
        {
            NvidiaGpuUsage = false;
        }

        if (OperatingSystem.IsLinux())
        {
            // AMD GPU usage is not yet supported on Linux
            AmdGpuUsage = false;
            // This is unnecessary on Linux
            NvidiaRefreshSettings = false;
        }
        else if (OperatingSystem.IsWindows())
        {
            AmdGpuUsage = true;
            IntelGpuUsage = false;
            NvidiaRefreshSettings = NvidiaGpuUsage;
        }

        // LibreHardwareMonitor does not support Intel GPUs
        IntelGpuUsage = false;

        BatteryInfo = CheckFeature(() =>
        {
            var batteryInfo =
                new BatteryInfo(computer.Hardware.First(hardware => hardware.HardwareType == HardwareType.Battery))
                {
                    UpdateVisitor = updateVisitor
                };
            batteryInfo.Refresh(1000);
        });
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
}