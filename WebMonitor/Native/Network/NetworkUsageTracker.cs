using System.Net.NetworkInformation;

namespace WebMonitor.Native.Network;

internal class NetworkUsageTracker : IRefreshable
{
    public List<NetworkUsage> Interfaces { get; }

    public NetworkUsageTracker()
    {
        Interfaces = GetInterfaces().ToList();
    }

    private static IEnumerable<NetworkUsage> GetInterfaces()
        => NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic
                => nic.NetworkInterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211)
            .Select(nic => new NetworkUsage(nic));

    public void Refresh(int millisSinceRefresh)
    {
        Interfaces.ForEach(nic => nic.Refresh(millisSinceRefresh));
    }
}