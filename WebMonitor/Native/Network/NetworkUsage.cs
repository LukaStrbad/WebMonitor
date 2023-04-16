using System.Net.NetworkInformation;

namespace WebMonitor.Native.Network;

public class NetworkUsage : IRefreshable
{
    /// <summary>
    /// NIC name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Download speed in bytes
    /// </summary>
    public long DownloadSpeed { get; private set; }

    /// <summary>
    /// Upload speed in bytes
    /// </summary>
    public long UploadSpeed { get; private set; }

    /// <summary>
    /// Total amount of data downloaded in bytes
    /// </summary>
    public long DataDownloaded { get; private set; }

    /// <summary>
    /// Total amount of data uploaded in bytes
    /// </summary>
    public long DataUploaded { get; private set; }

    /// <summary>
    /// NIC MAC address
    /// </summary>
    public string Mac { get; }

    private readonly NetworkInterface _nic;
    private IPInterfaceStatistics _lastIpStats;

    public NetworkUsage(NetworkInterface nic)
    {
        Name = nic.Name;
        Mac = nic.GetPhysicalAddress().ToString();
        _nic = nic;
        _lastIpStats = nic.GetIPStatistics();
    }

    public void Refresh(int millisSinceRefresh)
    {
        var ipStats = _nic.GetIPStatistics();

        DownloadSpeed = ipStats.BytesReceived - _lastIpStats.BytesReceived;
        UploadSpeed = ipStats.BytesSent - _lastIpStats.BytesSent;
        DataDownloaded = ipStats.BytesReceived;
        DataUploaded = ipStats.BytesSent;

        _lastIpStats = ipStats;
    }
}