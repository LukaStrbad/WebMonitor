namespace WebMonitor.Model;

public class ChangeAffinityRequest
{
    /// <summary>
    /// Process identifier
    /// </summary>
    public int Pid { get; set; }

    /// <summary>
    /// Thread number to change processor affinity for
    /// </summary>
    public int ThreadNumber { get; set; }

    /// <summary>
    /// Whether the thread should be active or not
    /// </summary>
    public bool On { get; set; }
}