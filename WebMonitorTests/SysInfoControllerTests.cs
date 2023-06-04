using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebMonitor;
using WebMonitor.Controllers;
using WebMonitor.Native;

namespace WebMonitorTests;

[Collection("io")]
public class SysInfoControllerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SysInfoController _controller;

    public SysInfoControllerTests()
    {
        _serviceProvider = new WebMonitorServiceProvider();
        _controller = new SysInfoController(_serviceProvider);
    }

    [Fact]
    public void RefreshInfo_ReturnsCorrectInterval()
    {
        var settings = _serviceProvider.GetRequiredService<Settings>();

        var result = _controller.RefreshInfo().Value;
        Assert.NotNull(result);
        Assert.Equal(settings.RefreshInterval, result.RefreshInterval);
    }

    [Fact]
    public void ComputerInfo_ReturnsValues()
    {
        var computerInfo = _controller.ComputerInfo().Value;

        Assert.NotNull(computerInfo);
        Assert.NotEmpty(computerInfo.Hostname);
        Assert.NotEmpty(computerInfo.CurrentUser);
        Assert.NotEmpty(computerInfo.OsName);
        Assert.NotEmpty(computerInfo.OsVersion);
        Assert.NotEmpty(computerInfo.OsBuild);

        var cpuInfo = computerInfo.Cpu;
        Assert.NotEmpty(cpuInfo.Name);
        Assert.NotEmpty(cpuInfo.Identifier);
        Assert.Equal(Environment.ProcessorCount, cpuInfo.NumThreads);
        // Some CPUs with hyper-threading have more than [threads / 2] number of cores such as 12/13th gen intel CPUs 
        Assert.InRange(cpuInfo.NumCores, cpuInfo.NumThreads / 2, cpuInfo.NumThreads);
        Assert.True(cpuInfo.BaseFrequencies.Count == cpuInfo.NumThreads);
        Assert.All(cpuInfo.BaseFrequencies, freq => Assert.True(freq > 0));

        var memoryInfo = computerInfo.Memory;
        Assert.True(memoryInfo.UsableMemory > 0);
        Assert.True(memoryInfo.TotalMemory > 0);
        Assert.True(memoryInfo.Speed > 0);
        Assert.True(memoryInfo.Voltage > 0);
        var sticks = memoryInfo.MemorySticks.ToList();
        Assert.NotEmpty(sticks);
        Assert.Equal(memoryInfo.TotalMemory, sticks.Select(ms => ms.Capacity).Aggregate((a, b) => a + b));

        var diskInfos = computerInfo.Disks.ToList();
        Assert.All(diskInfos, diskInfo =>
        {
            Assert.NotEmpty(diskInfo.Name);
            Assert.True(diskInfo.TotalSize > 0);
        });
    }

    [Fact]
    public void MemoryUsage_ReturnsValuesGreaterThanZero()
    {
        var memoryUsage = _controller.MemoryUsage().Value;

        Assert.NotNull(memoryUsage);
        Assert.True(memoryUsage.Total > 0);
        Assert.True(memoryUsage.Used > 0);
        Assert.InRange(memoryUsage.Used, 0, memoryUsage.Total);
        Assert.True(memoryUsage.Commited > 0);
    }

    [Fact]
    public void ProcessList_ReturnsValues()
    {
        var processList = _controller.ProcessList().Value?.ToList();

        Assert.NotNull(processList);
        Assert.NotEmpty(processList);
    }

    [Fact]
    public void CpuUsage_ReturnsValues()
    {
        var cpuUsage = _controller.CpuUsage().Value;

        Assert.NotNull(cpuUsage);
        Assert.True(cpuUsage.ThreadUsages.Count == Environment.ProcessorCount);
    }

    [Fact]
    public void GpuUsage_NotNull()
    {
        var gpuUsages = _controller.GpuUsages().Value;

        Assert.NotNull(gpuUsages);
        // We can't check if the collection is not empty because the system may not have a GPU,
        // or have drivers installed
    }

    [Fact]
    public void DiskUsages_ReturnsValues()
    {
        var diskUsages = _controller.DiskUsages().Value?.ToList();

        Assert.NotNull(diskUsages);
        Assert.NotEmpty(diskUsages);
    }

    [Fact]
    public void NetworkUsages_ReturnsValues()
    {
        var networkUsages = _controller.NetworkUsages().Value?.ToList();

        Assert.NotNull(networkUsages);
        Assert.NotEmpty(networkUsages);
    }
}