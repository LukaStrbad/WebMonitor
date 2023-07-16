namespace WebMonitor.Model;

public class ChangeAffinityRequest
{
    /// <summary>
    /// Process identifier
    /// </summary>
    public int Pid { get; set; }

    /// <summary>
    /// Information about the threads
    /// </summary>
    public List<ThreadInfo> Threads { get; set; } = new();

    /// <summary>
    /// Whether the thread should be active or not
    /// </summary>
    /// <param name="ThreadIndex">Thread index to change processor affinity for</param>
    /// <param name="On">Whether the thread should be active or not</param>
    public record ThreadInfo(int ThreadIndex, bool On);
}