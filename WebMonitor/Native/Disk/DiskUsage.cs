using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using WebMonitor.Native.Disk.Win32;

namespace WebMonitor.Native.Disk;

public class DiskUsage : IRefreshable
{
	/// <summary>
	/// Name of the disk
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }

	/// <summary>
	/// Current read speed in bytes
	/// </summary>
	[JsonPropertyName("readSpeed")]
	public long ReadSpeed { get; set; }

	/// <summary>
	/// Current read speed in bytes
	/// </summary>
	[JsonPropertyName("writeSpeed")]
	public long WriteSpeed { get; set; }

	/// <summary>
	/// Current disk utilization in %
	/// </summary>
	/// <value>A value between 0 and 100</value>
	/// [JsonPropertyName("utilization")]
	public float Utilization { get; set; }
		
	[SupportedOSPlatform("windows10.0")]
	private DiskStats _oldDiskStatsWin = default!;

	private readonly DiskInfo _diskInfo;

	public DiskUsage(DiskInfo diskInfo)
	{
		Name = diskInfo.Name;

		if (OperatingSystem.IsWindowsVersionAtLeast(10))
		{
			_oldDiskStatsWin = new DiskStats(diskInfo.DriveIndex);
		}

		_diskInfo = diskInfo;
	}

	public void Refresh(int millisSinceRefresh)
	{
		if (OperatingSystem.IsWindowsVersionAtLeast(10))
		{
			var newDiskStats = new DiskStats(_diskInfo.DriveIndex);
			var diskStats = newDiskStats - _oldDiskStatsWin;

			var rw = diskStats.ReadTime + diskStats.WriteTime;
			var utilization = (float)rw / (diskStats.IdleTime + rw) * 100f;

			Utilization = float.Clamp(utilization, 0, 100);
			ReadSpeed = diskStats.BytesRead;
			WriteSpeed = diskStats.BytesWritten;

			_oldDiskStatsWin = newDiskStats;
		}
	}
}
