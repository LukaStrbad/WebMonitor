namespace WebMonitorLib.Model;

public class ChangePtySizeRequest
{
    /// <summary>
    /// Session ID of the terminal.
    /// </summary>
    public int SessionId { get; set; }
    
    /// <summary>
    /// Number of columns in the terminal.
    /// </summary>
    public int Cols { get; set; }
    
    /// <summary>
    /// Number of rows in the terminal.
    /// </summary>
    public int Rows { get; set; }
}