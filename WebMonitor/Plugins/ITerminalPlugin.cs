using System.Net;

namespace WebMonitor.Plugins;

public interface ITerminalPlugin : IPlugin
{
    /// <summary>
    /// Starts a new session and returns a session ID.
    /// </summary>
    /// <returns>A session ID</returns>
    public int StartNewSession();
    
    /// <summary>
    /// Gets the port of the terminal with the given session ID.
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Session port</returns>
    public int GetPort(int sessionId);
    
    /// <summary>
    /// Changes the size of the terminal with the given session ID.
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cols">Number of columns</param>
    /// <param name="rows">Number of rows</param>
    public void ChangePtySize(int sessionId, int cols, int rows);
}