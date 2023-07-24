using System.Net.WebSockets;
using System.Text;
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
    private readonly ILogger<TerminalController> _logger;

    public TerminalController(IServiceProvider serviceProvider, ILogger<TerminalController> logger)
    {
        _pluginLoader = serviceProvider.GetRequiredService<PluginLoader>();
        _logger = logger;
    }

    [Route("/Terminal/session")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket accepted");
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task Echo(WebSocket webSocket)
    {
        var idBuffer = new byte[128];
        var response = await webSocket.ReceiveAsync(new Memory<byte>(idBuffer), CancellationToken.None);
        int sessionId;
        if (int.TryParse(Encoding.UTF8.GetString(idBuffer, 0, response.Count), out var parsedSessionId))
        {
            sessionId = parsedSessionId;
        }
        else
        {
            _logger.LogDebug("Invalid session id");
            var message = "Invalid session id"u8.ToArray();
            await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                CancellationToken.None);
            return;
        }

        var terminalPlugin = _pluginLoader.TerminalPlugin;

        if (terminalPlugin is null)
        {
            var message = "Terminal plugin not loaded or is not running."u8.ToArray();
            await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                CancellationToken.None);
            return;
        }

        var port = terminalPlugin.GetPort(sessionId);

        if (port == 0)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>("Invalid session id or session has been closed"u8.ToArray()),
                WebSocketMessageType.Text, true,
                CancellationToken.None);
            return;
        }

        // Client is used to proxy websocket requests
        var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://localhost:{port}"), CancellationToken.None);

        var task1 = Task.Run(async () =>
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await client.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    WebSocketMessageType.Text, true,
                    CancellationToken.None);

                receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        });

        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await client.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                WebSocketMessageType.Text,
                true, CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _logger.LogInformation("Closing WebSocket connection");
        try
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to close WebSocket connection");
        }
    }

    [HttpGet("isSessionAlive")]
    public ActionResult<bool> IsSessionAlive([FromQuery] int sessionId)
    {
        var port = _pluginLoader.TerminalPlugin?.GetPort(sessionId);
        
        if (port == null)
            return BadRequest();
        
        return Ok(port != 0);
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

        try
        {
            terminalPlugin.ChangePtySize(request.SessionId, request.Cols, request.Rows);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to change pty size");
            return BadRequest(e.Message);
        }
    }
}