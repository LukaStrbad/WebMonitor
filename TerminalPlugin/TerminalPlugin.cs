using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WebMonitor.Plugins;

namespace TerminalPlugin;

public class TerminalPlugin : ITerminalPlugin
{
    private Process? _process;

    public Version Version { get; } = new(1, 0);

    public string Name => "Terminal Plugin";
    public int? Port { get; private set; }

    public bool Start(string parentDirectory)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var fileName = OperatingSystem.IsWindows() ? "cmd.exe" : "bash";
        var indexJs = Path.Combine(parentDirectory, "index.js");
        var arguments = OperatingSystem.IsWindows() 
            ? $"/c node {indexJs} {Port}" 
            : $"""-c "node {indexJs} {Port}" """;

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
        _process.Start();

        var output = _process.StandardOutput.ReadLine();

        var result = output == $"Terminal server started on port {Port}";
        if (!result) 
            Port = null;

        return result;
    }

    public bool Stop()
    {
        try
        {
            _process?.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
