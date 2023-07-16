using System.Diagnostics;

namespace WebMonitor.Model;

public class ChangePriorityRequest
{
    /// <summary>
    /// Process identifier
    /// </summary>
    public int Pid { get; set; }
    
    /// <summary>
    /// New process priority
    /// </summary>
    public ProcessPriorityClass Priority { get; set; }
}