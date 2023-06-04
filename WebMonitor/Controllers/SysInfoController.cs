using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Model;
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
    private readonly Settings _settings;

    public SysInfoController(IServiceProvider serviceProvider)
    {
        _sysInfo = serviceProvider.GetRequiredService<SysInfo>();
        _settings = serviceProvider.GetRequiredService<Settings>();
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
    /// Sets SysInfo refresh interval
    /// </summary>
    /// <returns></returns>
    [HttpPost("refreshInterval")]
    public ActionResult ChangeRefreshInterval([FromBody] int interval)
    {
        if (interval < 1000)
            return BadRequest();

        _settings.RefreshInterval = interval;
        return Ok();
    }

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

    /// <summary>
    /// Fetches the current Nvidia refresh settings
    /// </summary>
    [HttpGet("nvidiaRefreshSettings")]
    public ActionResult<NvidiaRefreshSettings> NvidiaRefreshSetting() => _settings.NvidiaRefreshSettings;

    /// <summary>
    /// Sets the Nvidia refresh settings
    /// </summary>
    [HttpPost("nvidiaRefreshSettings")]
    public async Task<ActionResult> ChangeNvidiaRefreshSetting()
    {
        var settings = await JsonSerializer.DeserializeAsync<JsonObject>(Request.Body);
        if (settings == null)
            return BadRequest();

        var refreshSetting = settings["refreshSetting"]?.AsValue().GetValue<int>();
        var nRefreshIntervals = settings["nRefreshIntervals"]?.AsValue().GetValue<int>();

        if (refreshSetting == null || nRefreshIntervals == null)
            return BadRequest();

        _settings.NvidiaRefreshSettings.RefreshSetting = (NvidiaRefreshSetting)refreshSetting;
        _settings.NvidiaRefreshSettings.NRefreshIntervals = (int)nRefreshIntervals;
        return Ok();
    }
}