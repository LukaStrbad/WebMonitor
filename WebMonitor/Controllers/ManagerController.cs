using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    public ActionResult<ProcessPriorityClass?> ChangeProcessPriority([FromBody] ChangePriorityRequest request)
    {
        try
        {
            var priority = _manager.ChangePriority(request.Pid, request.Priority);
            return new OkObjectResult(priority);
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