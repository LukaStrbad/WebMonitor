using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Attributes;
using WebMonitor.Model;
using WebMonitor.Native;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ManagerController
{
    private readonly Manager _manager;

    public ManagerController(IServiceProvider serviceProvider)
    {
        _manager = serviceProvider.GetRequiredService<Manager>();
    }

    /// <summary>
    /// Kills a process by its PID
    /// </summary>
    /// <param name="pid">Process identifier</param>
    /// <returns>Name of the killed process</returns>
    [HttpPost("killProcess")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.Processes))]
    public ActionResult<string> KillProcess([FromBody] int pid)
    {
        try
        {
            var name = Manager.KillProcess(pid);
            return new OkObjectResult(name);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }
    
    /// <summary>
    /// Changes the priority of a process by its PID
    /// </summary>
    /// <param name="request">Object containing process ID and new priority</param>
    /// <returns>The set priority or null if not supported</returns>
    [HttpPost("changeProcessPriority")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.ProcessPriorityChange))]
    public ActionResult<ProcessPriorityClass?> ChangeProcessPriority([FromBody] ChangePriorityRequest request)
    {
        try
        {
            if (OperatingSystem.IsWindows() && request.PriorityWin is not null)
            {
                var priority = _manager.ChangePriorityWin(request.Pid, request.PriorityWin.Value);
                return new OkObjectResult(priority);
            }

            if (OperatingSystem.IsLinux() && request.PriorityLinux is not null)
            {
                var priority = _manager.ChangePriorityLinux(request.Pid, request.PriorityLinux.Value);
                return new OkObjectResult(priority);
            }

            return new BadRequestResult();
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }

    /// <summary>
    /// Changes the affinity of a process by its PID
    /// </summary>
    /// <param name="request">Object containing process ID nad thread indices and if the threads should be on or off</param>
    /// <returns>The set affinity of the process</returns>
    [HttpPost("changeProcessAffinity")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.ProcessAffinityChange))]
    public ActionResult<ulong> ChangeProcessAffinity([FromBody] ChangeAffinityRequest request)
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return new BadRequestObjectResult("This platform is unsupported");
        
        try
        {
            var affinity = _manager.ChangeAffinity(request.Pid, request.Threads);
            return new OkObjectResult(affinity);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }
}