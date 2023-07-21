using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommandLine;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebMonitor;
using WebMonitor.Native;
using WebMonitor.Options;
using WebMonitor.Plugins;

[assembly: InternalsVisibleTo("WebMonitorTests")]

var cmdOptions = Parser.Default.ParseArguments<CommandLineOptions>(args).Value;
if (cmdOptions is null)
    return;

// Setting the working directory to the location of the executable ensures that
// ASP.NET will correctly serve frontend files from the wwwroot folder.
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Load config
var config = Config.Load();

// Add addresses from command line
var addressInfos = AddressInfo.ParseFromStrings(cmdOptions.Ips);
config.Addresses.AddRange(addressInfos);

Console.WriteLine("Analyzing supported features...");
var supportedFeatures = new SupportedFeatures();

var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
// Initialize Settings and SysInfo early so they can start immediately
var settings = Settings.Load();
var sysInfo = new SysInfo(settings, version, supportedFeatures);
var manager = new Manager(supportedFeatures);

var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

var pluginLoader = new PluginLoader(loggerFactory.CreateLogger<PluginLoader>());
pluginLoader.Load();
supportedFeatures.ReevaluateWithPlugins(pluginLoader);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(sysInfo);
builder.Services.AddSingleton(supportedFeatures);
builder.Services.AddSingleton(manager);
builder.Services.AddSingleton(pluginLoader);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
    });
}

builder.Services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = long.MaxValue);
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.WebHost.UseKestrel(options =>
{
    foreach (var addressInfo in config.Addresses)
    {
        IPAddress ipAddress = default!;
        try
        {
            ipAddress = IPAddress.Parse(addressInfo.Ip);
        }
        catch (FormatException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Invalid ip address: \"{addressInfo.Ip}\"");
            Console.ResetColor();
            Environment.Exit(1);
        }

        options.Listen(ipAddress, addressInfo.Port, listenOptions =>
        {
            if (addressInfo.UseHttps)
                listenOptions.UseHttps();
        });
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseWebSockets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStopping.Register(() => { settings.Save(); });

app.Use(async (context, next) =>
{
    // If it's a WebSocket request at /terminal, proxy it to the backend
    if (context.WebSockets.IsWebSocketRequest && context.Request.Path == "/terminal")
    {
        Console.WriteLine("New WebSocket request");
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var terminalPlugin = pluginLoader.TerminalPlugin;
        
        if (terminalPlugin?.Port is null)
        {
            var message = "Terminal plugin not loaded or is not running."u8.ToArray();
            await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                CancellationToken.None);
            return;
        }
        
        // Client is used to proxy websocket requests
        var client = new ClientWebSocket();
        
        await client.ConnectAsync(new Uri($"ws://localhost:{terminalPlugin.Port}"), CancellationToken.None);
        
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
            await client.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count), WebSocketMessageType.Text,
                true, CancellationToken.None);
        
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        
        await task1;
    }
    else
    {
        await next(context);
    }
});

Console.WriteLine("Starting server...");
app.Run();