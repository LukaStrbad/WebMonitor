using LibreHardwareMonitor.Hardware;

namespace WebMonitorLib;

/// <summary>
/// Class that <c>LibreHardwareMonitor</c> uses to update values
/// </summary>
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (var subHardware in hardware.SubHardware) 
            subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}
