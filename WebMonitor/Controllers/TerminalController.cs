using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Attributes;
using WebMonitor.Model;
using WebMonitor.Plugins;
using WebMonitor.Utility;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class TerminalController : ControllerBase
{
    private readonly PluginLoader _pluginLoader;
    private readonly ILogger<TerminalController> _logger;
    private static readonly Dictionary<int, string?> Sessions = new();
    private readonly AuthUtility _authUtility;

    public TerminalController(IServiceProvider serviceProvider, ILogger<TerminalController> logger)
    {
        _pluginLoader = serviceProvider.GetRequiredService<PluginLoader>();
        _logger = logger;
        _authUtility = serviceProvider.GetRequiredService<AuthUtility>();
    }

    [Route("/Terminal/session")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Session()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket accepted");
            await SessionProxy(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    /// <summary>
    /// Method used to proxy websocket requests to the terminal plugin
    /// </summary>
    /// <param name="webSocket">WebSocket connection</param>
    private async Task SessionProxy(WebSocket webSocket)
    {
        var initBuffer = new byte[1024 * 4];
        // Read the JWT from the client
        var response = await webSocket.ReceiveAsync(new Memory<byte>(initBuffer), CancellationToken.None);
        var token = Encoding.UTF8.GetString(initBuffer, 0, response.Count);
        // Validate JWT
        var validateToken = _authUtility.ValidateToken(token);
        // If token is invalid, close the connection
        if (validateToken.Identity is not { IsAuthenticated: true })
        {
            _logger.LogDebug("Invalid token");
            var message = "Invalid token"u8.ToArray();
            await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                CancellationToken.None);
            return;
        }

        // Read the session id from the client
        response = await webSocket.ReceiveAsync(new Memory<byte>(initBuffer), CancellationToken.None);
        int sessionId;
        if (int.TryParse(Encoding.UTF8.GetString(initBuffer, 0, response.Count), out var parsedSessionId))
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

        var sessionUser = Sessions[sessionId];
        // If user is not the owner of the session, close the connection
        if (sessionUser is null || sessionUser != validateToken.Identity.Name)
        {
            _logger.LogInformation("User {Username} is not the owner of session {SessionId}",
                validateToken.Identity.Name, sessionId);
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

        // Get the port of the local websocket server
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

            // Read from the local websocket server and send to the client
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

        // Read from the client and send to the local websocket server
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

    /// <summary>
    /// Checks if the session is alive
    /// </summary>
    /// <param name="sessionId">ID of the session</param>
    [HttpGet("isSessionAlive")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.Terminal))]
    public ActionResult<bool> IsSessionAlive([FromQuery] int sessionId)
    {
        var port = _pluginLoader.TerminalPlugin?.GetPort(sessionId);

        var sessionUser = Sessions[sessionId];

        if (port is null || sessionUser is null || sessionUser != HttpContext.User.Identity?.Name)
            return Ok(false);

        if (port != 0)
            _logger.LogDebug("Session {SessionId} is alive", sessionId);
        else
            _logger.LogDebug("Session {SessionId} is not alive", sessionId);

        return Ok(port != 0);
    }

    /// <summary>
    /// Starts a new session
    /// </summary>
    /// <returns>ID of the newly created session</returns>
    [HttpGet("startNewSession")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.Terminal))]
    public ActionResult<int> StartNewSession()
    {
        var terminalPlugin = _pluginLoader.TerminalPlugin;

        if (terminalPlugin is null)
            return BadRequest();

        var sessionId = terminalPlugin.StartNewSession();
        Sessions.Add(sessionId, HttpContext.User.Identity?.Name);

        _logger.LogDebug("{Username} started a new session with id {SessionId}", HttpContext.User.Identity?.Name,
            sessionId);

        return Ok(sessionId);
    }

    /// <summary>
    /// Changes the size of the terminal on the server
    /// </summary>
    /// <param name="request">Session ID and new dimensions of the terminal</param>
    [HttpPost("changePtySize")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.Terminal))]
    public ActionResult ChangePtySize([FromBody] ChangePtySizeRequest request)
    {
        var terminalPlugin = _pluginLoader.TerminalPlugin;
        var sessionUser = Sessions[request.SessionId];

        if (terminalPlugin is null || sessionUser is null || sessionUser != HttpContext.User.Identity?.Name)
            return BadRequest();

        try
        {
            // Limit the minimum size of the terminal
            if (request.Cols < 10)
                request.Cols = 10;
            if (request.Rows < 5)
                request.Rows = 5;
            terminalPlugin.ChangePtySize(request.SessionId, request.Cols, request.Rows);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to change pty size");
            return BadRequest();
        }
    }
}