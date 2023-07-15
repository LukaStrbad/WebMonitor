using Microsoft.AspNetCore.Mvc;
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
}