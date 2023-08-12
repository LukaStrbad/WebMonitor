using System.Reflection;
using System.Text.Json;
using WebMonitor.Extensions;

namespace WebMonitor.Plugins;

/// <summary>
/// Class that contains all the logic for loading plugins
/// Currently only used for loading terminal plugin
/// </summary>
public class PluginLoader : IDisposable
{
    private readonly ILogger<PluginLoader> _logger;
    private ITerminalPlugin? _terminalPlugin;

    /// <summary>
    /// Directory where plugins are located
    /// </summary>
    public const string PluginDirectory = "plugins";

    /// <summary>
    /// Optional file that contains information about the plugin
    /// </summary>
    public const string PluginConfigFile = "plugin.json";

    public PluginLoader(ILogger<PluginLoader> logger)
    {
        _logger = logger;
    }

    public void Load()
    {
        Directory.CreateDirectory(PluginDirectory);
        LoadPluginsInDirectory(PluginDirectory);

        foreach (var dir in Directory.GetDirectories(PluginDirectory))
        {
            PluginConfig? pluginConfig = null;
            var pluginConfigPath = Path.Combine(dir, PluginConfigFile);

            // Check if plugin.json exists
            if (File.Exists(pluginConfigPath))
                pluginConfig = JsonSerializer.Deserialize<PluginConfig>(pluginConfigPath);

            LoadPluginsInDirectory(dir, pluginConfig);
        }

        if (_terminalPlugin is null)
        {
            _logger.LogInformation("Terminal plugin not found");
        }
    }

    private void LoadPluginsInDirectory(string directory, PluginConfig? pluginConfig = null)
    {
        var dlls = Directory.GetFiles(directory, "*.dll");
        // If plugin.json exists, only load the files specified in it
        if (pluginConfig is not null)
            dlls = pluginConfig.Files.Select(file => Path.Combine(directory, file)).ToArray();

        foreach (var dll in dlls)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // Check if type is a class derived from ITerminalPlugin and not abstract
                    if (type is not { IsClass: true, IsAbstract: false } ||
                        !typeof(ITerminalPlugin).IsAssignableFrom(type)) continue;

                    _terminalPlugin = (ITerminalPlugin?)Activator.CreateInstance(type);
                    if (_terminalPlugin is null)
                    {
                        _logger.LogError("Failed to create instance of plugin {Name} from file {FileName}",
                            type.Name, dll);
                        continue;
                    }

                    _logger.LogInformation("Loaded plugin '{Name}', version: {Version}", _terminalPlugin.Name,
                        _terminalPlugin.Version.ToShortString());
                    var result = _terminalPlugin.Start(directory);
                    if (result.Success) continue;
                    // If plugin failed to start, log the error and stop the plugin
                    _logger.LogError("Failed to start terminal plugin: {Message}", result.Message);
                    _terminalPlugin.Stop();
                    _terminalPlugin = null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load plugin {Dll}", dll);
            }
        }
    }

    public void Dispose()
    {
        _terminalPlugin?.Stop();
        _terminalPlugin = null;
        GC.SuppressFinalize(this);
    }

    public ITerminalPlugin? TerminalPlugin => _terminalPlugin;
}