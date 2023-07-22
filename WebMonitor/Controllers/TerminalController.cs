using Microsoft.AspNetCore.Mvc;
using WebMonitor.Model;
using WebMonitor.Plugins;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class TerminalController : ControllerBase
{
    private readonly PluginLoader _pluginLoader;

    public TerminalController(IServiceProvider serviceProvider)
    {
        _pluginLoader = serviceProvider.GetRequiredService<PluginLoader>();
    }

    [HttpGet("startNewSession")]
    public ActionResult<int> StartNewSession()
    {
        var terminalPlugin = _pluginLoader.TerminalPlugin;
        
        if (terminalPlugin is null)
            return BadRequest();
        
        return terminalPlugin.StartNewSession();
    }

    [HttpPost("changePtySize")]
    public ActionResult ChangePtySize([FromBody] ChangePtySizeRequest request)
    {
        var terminalPlugin = _pluginLoader.TerminalPlugin;

        if (terminalPlugin is null)
            return BadRequest();

        terminalPlugin.ChangePtySize(request.SessionId, request.Cols, request.Rows);
        return Ok();
    }
}