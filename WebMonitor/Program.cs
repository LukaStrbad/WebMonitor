using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CommandLine;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebMonitor;
using WebMonitor.Native;

[assembly: InternalsVisibleTo("WebMonitorTests")]

var cmdOptions = Parser.Default.ParseArguments<CommandLineOptions>(args).Value;
if (cmdOptions is null)
    return;

// Setting the working directory to the location of the executable ensures that
// ASP.NET will correctly serve frontend files from the wwwroot folder.
var executableLocation = Assembly.GetExecutingAssembly().Location;
var executableFile = new FileInfo(executableLocation);
if (executableFile.DirectoryName != null)
{
    Directory.SetCurrentDirectory(executableFile.DirectoryName);
}

var builder = WebApplication.CreateBuilder(args);

// Initialize Settings and SysInfo early so they can start immediately
var settings = Settings.Load();
var sysInfo = new SysInfo(settings);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(sysInfo);
builder.Services.AddSwaggerGen(options =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

builder.Services.Configure<KestrelServerOptions>(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.WebHost.UseKestrel(options =>
{
    if (!cmdOptions.Ips.Any()) return;
    
    foreach (var ip in cmdOptions.Ips)
    {
        var regex = IpRegex();
        var match = regex.Match(ip);
        var protocolGroup = match.Groups["protocol"];
        var ipGroup = match.Groups["ip"];
        var portGroup = match.Groups["port"];

        if (!ipGroup.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"""Invalid ip address: "{ip}" """);
            Console.ResetColor();
            Environment.Exit(1);
        }

        var ipAddress = IPAddress.Parse(ipGroup.Value);
        var useHttps = protocolGroup.Value == "https";
        // Parse won't fail because regex ensures that the port is a number.
        var port = portGroup.Success ? int.Parse(portGroup.Value) : 5000;

        options.Listen(ipAddress, port, listenOptions =>
        {
            if (useHttps)
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStopping.Register(() =>
{
    settings.Save();
});

app.Run();

internal partial class Program
{
    [GeneratedRegex("((?<protocol>https?)://)?(?<ip>(\\d{1,3}\\.){3}\\d{1,3})(:(?<port>\\d{1,5}))?")]
    private static partial Regex IpRegex();
}