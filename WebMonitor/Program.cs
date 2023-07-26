using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using CommandLine;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
var supportedFeatures = SupportedFeatures.Detect();

var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
// Initialize Settings and SysInfo early so they can start immediately
var settings = Settings.Load();
var sysInfo = new SysInfo(settings, version, supportedFeatures);
var manager = new Manager(supportedFeatures);
var db = new WebMonitorContext();

var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

var pluginLoader = new PluginLoader(loggerFactory.CreateLogger<PluginLoader>());
pluginLoader.Load();
supportedFeatures.ReevaluateWithPlugins(pluginLoader);

var programLogger = loggerFactory.CreateLogger<Program>();
if (cmdOptions.PromoteToAdmin is not null)
{
    var user = db.Users.FirstOrDefault(u => u.Username == cmdOptions.PromoteToAdmin);
    if (user is null)
    {
        programLogger.LogError("User {Username} not found", cmdOptions.PromoteToAdmin);
        return;
    }

    if (user.IsAdmin)
    {
        programLogger.LogInformation("User {Username} is already an admin", cmdOptions.PromoteToAdmin);
    }
    else
    {
        user.IsAdmin = true;
        db.SaveChanges();
        programLogger.LogInformation("User {Username} promoted to admin", cmdOptions.PromoteToAdmin);
    }
}
else
{
    programLogger.LogInformation("No user to promote to admin");
}

// Check if supported features have changed
foreach (var user in db.Users)
{
    if (user.IsAdmin)
    {
        user.AllowedFeatures = supportedFeatures;
        db.SaveChanges();
        continue;
    }

    var userSupportedFeatures = db.SupportedFeatures.FirstOrDefault(f => f.Id == user.AllowedFeaturesId) ??
                                new SupportedFeatures();
    foreach (var property in typeof(SupportedFeatures).GetProperties())
    {
        if (Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)) || property.PropertyType != typeof(bool))
            continue;

        var userValue = property.GetValue(userSupportedFeatures) as bool?;
        var value = property.GetValue(supportedFeatures) as bool?;

        if (userValue is true && value is false)
        {
            property.SetValue(userSupportedFeatures, false);
        }
    }

    user.AllowedFeatures = userSupportedFeatures;
    db.SaveChanges();
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtOptions = new JwtOptions(
    builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException(),
    builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException(),
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException())
);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(sysInfo);
builder.Services.AddSingleton(supportedFeatures);
builder.Services.AddSingleton(manager);
builder.Services.AddSingleton(pluginLoader);
builder.Services.AddSingleton(cmdOptions);
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton(db);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.Key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();
// builder.Services
//     .AddDefaultIdentity<IdentityUser>()
//     .AddRoles<IdentityRole>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStopping.Register(() => { settings.Save(); });

Console.WriteLine("Starting server...");
app.Run();