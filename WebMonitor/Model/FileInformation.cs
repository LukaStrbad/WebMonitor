namespace WebMonitor.Model;

public class FileInformation
{
    public string Name { get; }
    public string Path { get; }
    public ulong Size { get; }
    public DateTime LastModified { get; }
    public DateTime LastAccessed { get; }
    public DateTime Created { get; }

    public FileInformation(FileInfo fileInfo)
    {
        Name = fileInfo.Name;
        Path = fileInfo.FullName;
        Size = (ulong)fileInfo.Length;
        // Times are in UTC
        LastModified = fileInfo.LastWriteTimeUtc;
        LastAccessed = fileInfo.LastAccessTimeUtc;
        Created = fileInfo.CreationTimeUtc;
    }
}
