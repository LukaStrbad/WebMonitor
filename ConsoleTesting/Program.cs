using WebMonitorLib;
using WebMonitorLib.Settings;

var settings = Settings.Load(false);

var sysInfo = new SysInfo(settings, null, SupportedFeatures.Detect((_, _) => {}));

while (true)
{
    Thread.Sleep(1000);
    
    Console.WriteLine($"CPU Usage: {sysInfo.CpuUsage?.ThreadUsages.Average()}%");
}