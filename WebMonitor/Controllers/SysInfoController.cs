using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Attributes;
using WebMonitor.Model;
using WebMonitor.Native;
using WebMonitor.Native.Battery;
using WebMonitor.Native.Cpu;
using WebMonitor.Native.Disk;
using WebMonitor.Native.Gpu;
using WebMonitor.Native.Memory;
using WebMonitor.Native.Network;
using WebMonitor.Native.Process;
using WebMonitor.Native.Process.Linux;
using WebMonitor.Native.Process.Win;
using WebMonitor.Options;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SysInfoController : ControllerBase
{
    private readonly SysInfo _sysInfo;
    private readonly Settings _settings;
    private readonly SupportedFeatures _supportedFeatures;

    public SysInfoController(IServiceProvider serviceProvider)
    {
        _sysInfo = serviceProvider.GetRequiredService<SysInfo>();
        _settings = serviceProvider.GetRequiredService<Settings>();
        _supportedFeatures = serviceProvider.GetRequiredService<SupportedFeatures>();
    }

    /// <summary>
    /// Returns the current version of WebMonitor
    /// </summary>
    [HttpGet("version")]
    public ActionResult<string?> Version() => _sysInfo.Version;

    /// <summary>
    /// Returns server supported features
    /// </summary>
    [HttpGet("supportedFeatures")]
    public ActionResult<SupportedFeatures> SupportedFeatures()
    {
        return _supportedFeatures;
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
    /// Returns milliseconds since Unix epoch when the settings were last updated
    /// </summary>
    /// <remarks>Time is in UTC</remarks>
    [HttpGet("settingsUpdateTime")]
    public ActionResult<long> SettingsUpdateTime() => _settings.LastUpdateTime;

    /// <summary>
    /// Sets SysInfo refresh interval
    /// </summary>
    /// <returns></returns>
    [HttpPost("refreshInterval")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.RefreshIntervalChange))]
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
    [Authorize]
    public ActionResult<ComputerInfo> ComputerInfo() => _sysInfo.ComputerInfo;

    /// <summary>
    /// Fetches current memory usage
    /// </summary>
    [HttpGet("memoryUsage")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.MemoryUsage))]
    public ActionResult<MemoryUsage?> MemoryUsage() => _sysInfo.MemoryUsage;

    /// <summary>
    /// Fetches data about currently running processes
    /// </summary>
    [HttpGet("processList")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.Processes))]
    public ActionResult<IEnumerable<ProcessInfo>?> ProcessList() => _sysInfo.Processes?.ToList();

    /// <summary>
    /// Fetches per thread usage
    /// </summary>
    [HttpGet("cpuUsage")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.CpuUsage))]
    public ActionResult<CpuUsage?> CpuUsage() => _sysInfo.CpuUsage;

    /// <summary>
    /// Fetches GPU usages
    /// </summary>
    /// <returns></returns>
    [HttpGet("gpuUsages")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.GpuUsage))]
    public ActionResult<IEnumerable<IGpuUsage>?> GpuUsages() => _sysInfo.GpuUsages?.ToList();

    /// <summary>
    /// Fetches disk usages
    /// </summary>
    [HttpGet("diskUsages")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.DiskUsage))]
    public ActionResult<IEnumerable<DiskUsage>?> DiskUsages() => _sysInfo.DiskUsages;

    /// <summary>
    /// Fetches usage statistics per NIC
    /// </summary>
    [HttpGet("networkUsages")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.NetworkUsage))]
    public ActionResult<IEnumerable<NetworkUsage>?> NetworkUsages() => _sysInfo.NetworkUsages?.ToList();

    /// <summary>
    /// Fetches the current Nvidia refresh settings
    /// </summary>
    [HttpGet("nvidiaRefreshSettings")]
    [Authorize]
    public ActionResult<NvidiaRefreshSettings> NvidiaRefreshSetting() => _settings.NvidiaRefreshSettings;

    /// <summary>
    /// Sets the Nvidia refresh settings
    /// </summary>
    [HttpPost("nvidiaRefreshSettings")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.NvidiaRefreshSettings))]
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

    /// <summary>
    /// Fetches info about the battery
    /// </summary>
    [HttpGet("batteryInfo")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.BatteryInfo))]
    public ActionResult<BatteryInfo?> BatteryInfo() => _sysInfo.BatteryInfo;

    [HttpGet("extendedProcessInfo")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.ExtendedProcessInfo))]
    public ActionResult<ExtendedProcessInfo> ExtendedProcessInfo([FromQuery] int pid)
    {
        try
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
            {
                var processInfo = new ExtendedProcessInfoWin(pid);
                return new OkObjectResult(processInfo);
            }

            if (OperatingSystem.IsLinux())
            {
                var processInfo = new ExtendedProcessInfoLinux(pid);
                // This will throw an exception if the user is not authorized to read the process info
                var handleCount = processInfo.HandleCount;
                return new OkObjectResult(processInfo);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }

        return new BadRequestResult();
    }
}
