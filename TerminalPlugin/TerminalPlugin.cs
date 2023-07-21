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

        var fileName = OperatingSystem.IsWindows() ? @"cmd.exe" : "bash";
        var arguments = OperatingSystem.IsWindows() ? "/c node" : "-c node";
        var indexJs = Path.Combine(parentDirectory, "index.js");

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"{arguments} {indexJs} {Port}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
        _process.Start();

        var output = _process.StandardOutput.ReadLine();

        return output == $"Terminal server started on port {Port}";
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
