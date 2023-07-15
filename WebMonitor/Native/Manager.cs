using SystemProcess = System.Diagnostics.Process;

namespace WebMonitor.Native;

public class Manager
{
    private readonly SupportedFeatures _supportedFeatures;

    public Manager(SupportedFeatures supportedFeatures)
    {
        _supportedFeatures = supportedFeatures;
    }

    public static string KillProcess(int pid)
    {
        var process = SystemProcess.GetProcessById(pid);
        var name = process.ProcessName;
        process.Kill();
        return name;
    }
}