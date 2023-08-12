using System.Management;
using System.Runtime.Versioning;

namespace WebMonitor.Extensions;

public static class ManagementBaseObjectExtensions
{
    [SupportedOSPlatform("windows")]
    public static bool TryGetValue<T>(this ManagementBaseObject managementBaseObject, string key, out T value)
    {
        try
        {
            value = (T)managementBaseObject[key];
            return true;
        }
        catch
        {
            value = default!;
            return false;
        }
    }
}