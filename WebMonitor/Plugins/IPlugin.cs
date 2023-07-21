namespace WebMonitor.Plugins;

public interface IPlugin
{
    /// <summary>
    /// Plugin version
    /// </summary>
    public Version Version { get; }
    
    /// <summary>
    /// Plugin name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Function that is called when the plugin is loaded
    /// </summary>
    /// <param name="parentDirectory">Directory where the plugin is located</param>
    /// <returns>true if plugin has been started successfully</returns>
    public bool Start(string parentDirectory) => true;

    /// <summary>
    /// Function that is called when the plugin is unloaded
    /// </summary>
    /// <returns>true if plugin has stopped successfully</returns>
    public bool Stop() => true;
}