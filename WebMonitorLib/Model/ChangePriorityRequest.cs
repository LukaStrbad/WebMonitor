using System.Diagnostics;

namespace WebMonitorLib.Model;

public class ChangePriorityRequest
{
    /// <summary>
    /// Process identifier
    /// </summary>
    public int Pid { get; set; }
    
    /// <summary>
    /// New process priority on Windows
    /// </summary>
    public ProcessPriorityClass? PriorityWin { get; set; }
    
    /// <summary>
    /// New process priority on Linux
    /// </summary>
    public int? PriorityLinux { get; set; }
}