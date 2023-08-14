using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using WebMonitor.Plugins;

namespace TerminalPlugin;

public class TerminalPlugin : ITerminalPlugin
{
    private const string NodeVersionFile = "node-version.txt";
    private string _parentDirectory = null!;

    private Process? _process;

    // Get version from AssemblyVersion
    public Version Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version!;
        }
    }

    public string Name => "Terminal Plugin";

    private readonly Dictionary<int, (Process Process, int Port)> _sessions = new();
    private int _nextSessionId = 0;

    public (bool Success, string? Message) Start(string parentDirectory)
    {
        _parentDirectory = parentDirectory;
        var nodeVersion = GetNodeVersion();
        if (nodeVersion is null)
            return (false, "Node.js is not installed");

        try
        {
            // Load the compiled node version
            var versionFromFile = File.ReadAllText(Path.Combine(parentDirectory, NodeVersionFile));
            var pluginNodeVersion = new Version(versionFromFile);

            return nodeVersion.Major == pluginNodeVersion.Major
                ? (true, null)
                : (false, $"Node.js version {pluginNodeVersion.Major}.x.x is required, current version: {nodeVersion}");
        }
        catch
        {
            return (false, $"{NodeVersionFile} doesn't exist!");
        }

    }

    private static Version? GetNodeVersion()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var output = "";
        var task = Task.Run(() => output = process.StandardOutput.ReadLine());
        process.WaitForExit();

        // Wait for the task to complete or timeout after 2 seconds
        task.Wait(TimeSpan.FromSeconds(2));
        if (!task.IsCompletedSuccessfully)
            return null;

        // Remove leading 'v'
        return Version.TryParse(output[1..], out var version) ? version : null;
    }

    private static int GetOpenPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public bool Stop()
    {
        foreach (var session in _sessions)
        {
            try
            {
                session.Value.Process.Kill();
            }
            catch
            {
                // ignored
            }
        }

        return true;
    }

    public int StartNewSession()
    {
        var port = GetOpenPort();
        var shell = OperatingSystem.IsWindows() ? "powershell.exe" : "bash";
        var indexJs = Path.Combine(_parentDirectory, "index.js");

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"{indexJs} {shell} {port}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            }
        };
        _process.Start();

        var output = _process.StandardOutput.ReadLine();
        var sessionId = _nextSessionId++;

        if (output == $"Terminal server started on port {port}")
            _sessions.Add(sessionId, (_process, port));
        else
            throw new Exception($"Failed to start terminal server: {output}");

        return sessionId;
    }

    public int GetPort(int sessionId)
    {
        var session = _sessions[sessionId];
        if (session.Process.HasExited)
            return 0;
        return session.Port;
    }

    public void ChangePtySize(int sessionId, int cols, int rows)
    {
        var process = _sessions[sessionId].Process;
        if (process.HasExited)
            throw new Exception("Session has exited");
        process.StandardInput.WriteLine($"resize: {cols} {rows}");
    }
}
