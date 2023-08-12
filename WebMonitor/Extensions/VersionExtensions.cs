using System.Text;

namespace WebMonitor.Extensions;

public static class VersionExtensions
{
    /// <summary>
    /// Formats the version to a short string. If the build or revision is 0, it will be omitted
    /// </summary>
    /// <param name="version">This Version object</param>
    public static string ToShortString(this Version version)
    {
        var sb = new StringBuilder();
        
        sb.Append($"{version.Major}.{version.Minor}");
        // If version components are not specified in the Version constructor, they will be -1
        if (version is { Build: > 0, Revision: > 0 })
            sb.Append($".{version.Build}.{version.Revision}");
        else if (version is { Build: > 0, Revision: <= 0 })
            sb.Append($".{version.Build}");
        else if (version is { Build: <= 0, Revision: > 0 })
            sb.Append($".0.{version.Revision}");
        
        return sb.ToString();
    }
}