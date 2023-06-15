using WebMonitor.Native;
using WebMonitor.Options;

namespace WebMonitorTests;

public class WebMonitorServiceProvider : IServiceProvider
{
    private readonly Settings _settings;
    private readonly SysInfo _sysInfo;

    public WebMonitorServiceProvider()
    {
        _settings = Settings.Load();
        _sysInfo = new SysInfo(_settings, null);
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == _settings.GetType())
            return _settings;
        if (serviceType == _sysInfo.GetType())
            return _sysInfo;

        return null;
    }
}