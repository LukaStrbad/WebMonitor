using Microsoft.AspNetCore.Mvc;
using WebMonitor.Native;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Gpu;
using WebMonitor.Native.Memory;
using WebMonitor.Native.Network;
using WebMonitor.Native.Process;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SysInfoController : ControllerBase
{
    private readonly SysInfo _sysInfo;

    public SysInfoController(IServiceProvider serviceProvider)
    {
        _sysInfo = serviceProvider.GetService<SysInfo>()!;
    }

    /// <summary>
    /// Returns the client IP address
    /// </summary>
    [HttpGet("clientIP")]
    public ActionResult<string?> ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// Returns the number of milliseconds since the last refresh
    /// </summary>
	[HttpGet("refreshInfo")]
    public ActionResult<RefreshInformation> RefreshInfo() => _sysInfo.RefreshInfo;

    /// <summary>
    /// Fetches basic computer info
    /// </summary>
    [HttpGet("computerInfo")]
    public ActionResult<ComputerInfo> ComputerInfo() => _sysInfo.ComputerInfo;

    /// <summary>
    /// Fetches current memory usage
    /// </summary>
    [HttpGet("memoryUsage")]
    public ActionResult<MemoryUsage> MemoryUsage() => _sysInfo.MemoryUsage;

    /// <summary>
    /// Fetches data about currently running processes
    /// </summary>
    [HttpGet("processList")]
    public ActionResult<IEnumerable<ProcessInfo>> ProcessList() => _sysInfo.Processes.ToList();

    /// <summary>
    /// Fetches per thread usage
    /// </summary>
    [HttpGet("cpuUsage")]
    public ActionResult<CpuUsage> CpuUsage() => _sysInfo.CpuUsage;

    /// <summary>
    /// Fetches GPU usages
    /// </summary>
    /// <returns></returns>
    [HttpGet("gpuUsages")]
    public ActionResult<IEnumerable<IGpuUsage>> GpuUsages() => _sysInfo.GpuUsages.ToList();

    /// <summary>
    /// Fetches disk usages
    /// </summary>
    [HttpGet("diskUsages")]
    public ActionResult<IEnumerable<DiskUsage>> DiskUsages() => _sysInfo.DiskUsages;

    /// <summary>
    /// Fetches usage statistics per NIC
    /// </summary>
    [HttpGet("networkUsages")]
    public ActionResult<IEnumerable<NetworkUsage>> NetworkUsages() => _sysInfo.NetworkUsages.ToList();
}
