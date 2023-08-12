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
    /// <returns>A tuple indicating success and an optional message</returns>
    public (bool Success, string? Message) Start(string parentDirectory) => (true, null);
    /// <summary>
    /// Function that is called when the plugin is unloaded
    /// </summary>
    /// <returns>true if plugin has stopped successfully</returns>
    public bool Stop() => true;
}