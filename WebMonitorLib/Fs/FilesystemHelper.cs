namespace WebMonitorLib.Fs;

public static class FilesystemHelper
{
    /// <summary>
    /// Returns root directories of the system
    /// </summary>
    /// <returns>List of root directories of the system</returns>
    /// <exception cref="FsException">Thrown when directory could not be read</exception>
    public static IEnumerable<FileOrDir> GetRootDirs()
    {
        if (OperatingSystem.IsLinux())
        {
            // On Linux hardcode folders to / and ~
            return
            [
                FileOrDir.Dir(new DirectoryInfo("/")),
                FileOrDir.Dir(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
            ];
        }

        var rootDirs = new List<FileOrDir>();
        foreach (var driveInfo in DriveInfo.GetDrives())
        {
            try
            {
                rootDirs.Add(FileOrDir.Dir(driveInfo.RootDirectory));
            }
            catch (Exception e)
            {
                throw new FsException("Failed to read drive", e);
            }
        }

        return rootDirs;
    }
}