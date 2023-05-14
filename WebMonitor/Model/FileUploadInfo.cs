namespace WebMonitor.Model;

public class FileUploadInfo
{
    public string FileName { get; }

    public bool Success { get; }

    public FileUploadInfo(string fileName, bool success)
    {
        FileName = fileName;
        Success = success;
    }
}
