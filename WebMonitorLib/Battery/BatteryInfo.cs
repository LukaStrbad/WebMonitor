using System.Text.Json.Serialization;
using LibreHardwareMonitor.Hardware;

namespace WebMonitorLib.Battery;

public class BatteryInfo : IRefreshable
{
    private readonly IHardware _battery;
    private readonly ISensor? _chargeLevelSensor;
    private readonly ISensor? _voltageSensor;
    private readonly ISensor? _currentSensor;
    private readonly ISensor? _powerSensor;
    private readonly ISensor? _fullCapacitySensor;
    private readonly ISensor? _designCapacitySensor;
    private readonly ISensor? _currentCapacitySensor;

    /// <summary>
    /// Charge level as a percentage (0 - 100)
    /// </summary>
    public float ChargeLevel => _chargeLevelSensor?.Value ?? 0f;

    /// <summary>
    /// Voltage in mV
    /// </summary>
    public uint? Voltage => (uint?)(_voltageSensor?.Value * 1000f);

    /// <summary>
    /// Current in A
    /// </summary>
    public float? Current
    {
        get
        {
            if (_currentSensor is null)
                return null;
            
            if (_currentSensor.Name == "Charge Current")
                return (uint?)(_currentSensor.Value * 1000f);
            
            // Negative value means discharging
            return -(uint?)(_currentSensor?.Value * 1000f);
        }
    }

    /// <summary>
    /// Power in W
    /// </summary>
    public float? Power
    {
        get
        {
            if (_powerSensor is null)
                return null;

            if (_powerSensor.Name == "Charge Rate")
                return (uint?)(_powerSensor.Value * 1000f);
            
            // Negative value means discharging
            return -(uint?)(_powerSensor?.Value * 1000f);
        }
    }

    /// <summary>
    /// Full capacity in mWh
    /// </summary>
    public uint? FullCapacity => (uint?)_fullCapacitySensor?.Value;

    /// <summary>
    /// Design capacity in mWh
    /// </summary>
    public uint? DesignCapacity => (uint?)_designCapacitySensor?.Value;

    /// <summary>
    /// Current capacity in mWh
    /// </summary>
    public uint? CurrentCapacity => (uint?)_currentCapacitySensor?.Value;

    [JsonIgnore]
    internal UpdateVisitor? UpdateVisitor { get; init; }


    public BatteryInfo(IHardware battery)
    {
        _battery = battery;

        _chargeLevelSensor = Array.Find(battery.Sensors, s =>
            s.SensorType is SensorType.Level
            && s.Name.Contains("Charge Level"));

        _voltageSensor = Array.Find(battery.Sensors, s => s.SensorType is SensorType.Voltage);
        _currentSensor = Array.Find(battery.Sensors, s => s.SensorType is SensorType.Current);
        _powerSensor = Array.Find(battery.Sensors, s => s.SensorType is SensorType.Power);
        
        _fullCapacitySensor = Array.Find(battery.Sensors, s => 
            s.SensorType is SensorType.Energy
            && s.Name.Contains("Full Charged Capacity"));

        _designCapacitySensor = Array.Find(battery.Sensors, s =>
            s.SensorType is SensorType.Energy
            && s.Name.Contains("Designed Capacity"));

        _currentCapacitySensor = Array.Find(battery.Sensors, s =>
            s.SensorType is SensorType.Energy
            && s.Name.Contains("Remaining Capacity"));
    }

    public void Refresh(int millisSinceRefresh)
    {
        if (UpdateVisitor is null)
            return;
        
        _battery.Accept(UpdateVisitor);
    }
}