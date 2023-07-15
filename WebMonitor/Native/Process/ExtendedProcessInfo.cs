using System.Runtime.Versioning;

namespace WebMonitor.Native.Process;

public abstract class ExtendedProcessInfo
{
    protected readonly System.Diagnostics.Process Process;
    
    /// <summary>
    /// Process ID
    /// </summary>
    public int Pid { get; init; }
    
    /// <summary>
    /// Process name
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Process owner username
    /// </summary>
    /// <remarks>Null is unknown user</remarks>
    public string? Owner { get; init; }
    
    /// <summary>
    /// Process affinity
    /// </summary>
    public ulong Affinity { get; init; }

    /// <summary>
    /// Process working set
    /// </summary>
    public long WorkingSet => Process.WorkingSet64;
    
    /// <summary>
    /// Process peak working set
    /// </summary>
    public long PeakWorkingSet => Process.PeakWorkingSet64;
    
    /// <summary>
    /// Process paged memory
    /// </summary>
    public long PagedMemory => Process.PagedMemorySize64;
    
    /// <summary>
    /// Process peak paged memory
    /// </summary>
    public long PeakPagedMemory => Process.PeakPagedMemorySize64;
    
    /// <summary>
    /// Process private memory
    /// </summary>
    public long PrivateMemory => Process.PrivateMemorySize64;

    /// <summary>
    /// Process virtual memory
    /// </summary>
    public long VirtualMemory => Process.VirtualMemorySize64;
    
    /// <summary>
    /// Process peak virtual memory
    /// </summary>
    public long PeakVirtualMemory => Process.PeakVirtualMemorySize64;
    
    /// <summary>
    /// Process number of threads
    /// </summary>
    public int ThreadCount => Process.Threads.Count;
    
    /// <summary>
    /// Process number of handles
    /// </summary>
    public int HandleCount => Process.HandleCount;

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    protected ExtendedProcessInfo(int pid)
    {
        Process = System.Diagnostics.Process.GetProcessById(pid);
        Pid = pid;
        Name = Process.ProcessName;
        Affinity = (ulong)Process.ProcessorAffinity;
    }
}