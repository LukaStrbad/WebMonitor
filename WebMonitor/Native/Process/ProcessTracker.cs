using SystemProcess = System.Diagnostics.Process;

namespace WebMonitor.Native.Process;

/// <summary>
/// A class that tracks CPU and disk usage (WIP) of each process
/// </summary>
internal class ProcessTracker : IRefreshable
{
    private readonly Dictionary<int, TimeSpan> _previousCpu;

    public Dictionary<int, ProcessInfo> Processes { get; }

    public ProcessTracker()
    {
        _previousCpu = new Dictionary<int, TimeSpan>(255);
        Processes = new Dictionary<int, ProcessInfo>(255);
    }

    public void Refresh(int millisSinceRefresh)
    {
        Processes.Clear();
        foreach (var process in SystemProcess.GetProcesses())
        {
            try
            {
                if (_previousCpu.TryGetValue(process.Id, out var cpuTime))
                {
                    _previousCpu.Remove(process.Id);
                    _previousCpu.Add(process.Id, process.TotalProcessorTime);

                    var timeDiff = process.TotalProcessorTime - cpuTime;

                    Processes[process.Id] = ProcessToProcessInfo(process, millisSinceRefresh,
                        timeDiff.TotalMilliseconds);
                }
                else
                {
                    _previousCpu.Remove(process.Id);
                    _previousCpu.Add(process.Id, process.TotalProcessorTime);
                    Processes[process.Id] = ProcessToProcessInfo(process, millisSinceRefresh, 0);
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    private static ProcessInfo ProcessToProcessInfo(SystemProcess process, int millisSinceRefresh, double workingMillis)
    {
        return new ProcessInfo
        {
            Pid = process.Id,
            Name = process.ProcessName,
            MemoryUsage = process.PrivateMemorySize64,
            DiskUsage = 0,
            CpuUsage = (float)(workingMillis / millisSinceRefresh * 100 / Environment.ProcessorCount)
        };
    }
}