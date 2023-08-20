using WebMonitor;
using WebMonitor.Native;
using WebMonitor.Options;
using WebMonitor.Utility;

namespace WebMonitorTests;

public class WebMonitorServiceProvider : IServiceProvider
{
    private readonly Settings _settings;
    private readonly SysInfo _sysInfo;
    private readonly SupportedFeatures _supportedFeatures;
    private readonly AuthUtility _authUtility;

    public WebMonitorServiceProvider()
    {
        _supportedFeatures = new SupportedFeatures();
        _settings = Settings.Load();
        _sysInfo = new SysInfo(_settings, null, _supportedFeatures);
        var jwtOptions = new JwtOptions("http://localhost:5000", "http://localhost:5000",
            "This is a sample key for demo purposes"u8.ToArray());
        _authUtility = new AuthUtility(jwtOptions);
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == _settings.GetType())
            return _settings;
        if (serviceType == _sysInfo.GetType())
            return _sysInfo;
        if (serviceType == _supportedFeatures.GetType())
            return _supportedFeatures;
        if (serviceType == _authUtility.GetType())
            return _authUtility;

        return null;
    }
}