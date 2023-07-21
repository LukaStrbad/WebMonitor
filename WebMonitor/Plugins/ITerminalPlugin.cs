using System.Net;

namespace WebMonitor.Plugins;

public interface ITerminalPlugin : IPlugin
{
    /// <summary>
    /// Port of the server that plugin is connected to
    /// </summary>
    public int? Port { get; }
}