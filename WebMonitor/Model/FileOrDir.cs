namespace WebMonitor.Model;

public sealed class FileOrDir
{
    public string Type { get; }

    public string Path { get; }

    public long? Size { get; }

    public int? ChildrenCount { get; }

    private FileOrDir(string type, string path, long? size = null, int? childrenCount = null)
    {
        Type = type;
        Path = path;
        Size = size;
        ChildrenCount = childrenCount;
    }

    public static FileOrDir Dir(DirectoryInfo dir)
    {
        int childrenCount;

        try
        {
            childrenCount = dir.GetFileSystemInfos().Length;
        }
        catch (UnauthorizedAccessException)
        {
            childrenCount = -1;
        }

        return new("dir", dir.FullName, childrenCount: childrenCount);
    }
    public static FileOrDir File(FileInfo file) => new("file", file.FullName, file.Length);
}
