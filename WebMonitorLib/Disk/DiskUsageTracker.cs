namespace WebMonitorLib.Disk;

internal class DiskUsageTracker : IRefreshable
{
	public List<DiskUsage> DiskUsages { get; }

	public DiskUsageTracker()
	{
		DiskUsages = DiskInfo
			.GetDiskInfos()
			.Select(di => new DiskUsage(di))
			.ToList();
	}

	public void Refresh(int millisSinceRefresh)
	{
		DiskUsages.ForEach(du => du.Refresh(millisSinceRefresh));
	}
}
