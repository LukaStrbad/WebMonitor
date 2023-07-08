using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebMonitor;
using WebMonitor.Native;
using WebMonitor.Options;

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

var supportedFeatures = new SupportedFeatures();
Console.WriteLine(JsonSerializer.Serialize(supportedFeatures, new JsonSerializerOptions { WriteIndented = true }));

var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
// Initialize Settings and SysInfo early so they can start immediately
var settings = Settings.Load();
var sysInfo = new SysInfo(settings, version, supportedFeatures);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(sysInfo);
builder.Services.AddSingleton(supportedFeatures);

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStopping.Register(() => { settings.Save(); });

app.Run();