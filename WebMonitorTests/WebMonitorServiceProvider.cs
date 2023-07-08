using WebMonitor;
using WebMonitor.Native;
using WebMonitor.Options;

namespace WebMonitorTests;

public class WebMonitorServiceProvider : IServiceProvider
{
    private readonly Settings _settings;
    private readonly SysInfo _sysInfo;
    private readonly SupportedFeatures _supportedFeatures;

    public WebMonitorServiceProvider()
    {
        _supportedFeatures = new SupportedFeatures();
        _settings = Settings.Load();
        _sysInfo = new SysInfo(_settings, null, _supportedFeatures);
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == _settings.GetType())
            return _settings;
        if (serviceType == _sysInfo.GetType())
            return _sysInfo;
        if (serviceType == _supportedFeatures.GetType())
            return _supportedFeatures;

        return null;
    }
}