using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using Windows.Win32;

namespace WebMonitor.Native.Memory;

public class MemoryInfo
{
    /// <summary>
    /// Usable memory in bytes
    /// </summary>
    public ulong UsableMemory { get; private set; }

    /// <summary>
    /// Reserved memory in bytes
    /// </summary>
    public ulong ReservedMemory { get; private set; }

    /// <summary>
    /// Total installed system memory
    /// </summary>
    public ulong TotalMemory { get; private set; }

    /// <summary>
    /// Memory speed in MHz
    /// </summary>
    public uint Speed { get; private set; }

    /// <summary>
    /// Memory voltage in mV
    /// </summary>
    public uint Voltage { get; private set; }

    /// <summary>
    /// RAM form factor (e.g. DIMM)
    /// </summary>
    public string? FormFactor { get; private set; }

    /// <summary>
    /// RAM type (e.g. DDR4)
    /// </summary>
    public string? Type { get; private set; }

    /// <summary>
    /// Info about each memory stick
    /// </summary>
    public IEnumerable<MemoryStickInfo> MemorySticks { get; private set; } = Enumerable.Empty<MemoryStickInfo>();

    public MemoryInfo()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6))
        {
            InitWindows();
        }
        else if (OperatingSystem.IsLinux())
        {
            InitLinux();
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    // Supported since Windows Vista
    [SupportedOSPlatform("windows6.0")]
    private void InitWindows()
    {
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");

        var memorySticks = new List<MemoryStickInfo>();
        var firstLoop = true;
        foreach (var obj in searcher.Get())
        {
            if (firstLoop)
            {
                // These values are the same for all memory sticks
                Speed = (uint)obj["Speed"];
                Voltage = (uint)obj["ConfiguredVoltage"];
                FormFactor = GetMemoryFormFactor((ushort)obj["FormFactor"]);
                Type = GetMemoryType((uint)obj["SMBIOSMemoryType"]);
                firstLoop = false;
            }

            var manufacturer = obj["Manufacturer"].ToString()?.Trim();
            var partNumber = obj["PartNumber"].ToString()?.Trim();
            var capacity = (ulong)obj["Capacity"];
            memorySticks.Add(new MemoryStickInfo(manufacturer, partNumber, capacity));
        }

        MemorySticks = memorySticks;
        PInvoke.GlobalMemoryStatus(out var memoryStatus);
        UsableMemory = memoryStatus.dwTotalPhys;
        // There is no sum method for ulong
        TotalMemory = memorySticks
            .Select(ms => ms.Capacity)
            .Aggregate((a, b) => a + b);
        ReservedMemory = TotalMemory - memoryStatus.dwTotalPhys;
    }

    [SupportedOSPlatform("linux")]
    private void InitLinux()
    {
        var meminfo = File
            .ReadAllLines("/proc/meminfo")
            .Select(line => line.Split(':'))
            .ToDictionary(split => split[0].Trim(), split => split[1].Trim());

        // Values are shown in kB, so we need to remove the suffix and multiply by 1024
        UsableMemory = ulong.Parse(meminfo["MemTotal"].Split(' ')[0]) * 1024;
        ReservedMemory = ulong.Parse(meminfo["DirectMap1G"].Split(' ')[0]) * 1024;
        TotalMemory = UsableMemory + ReservedMemory;

        var dmidecodeOutput = GetDmidecodeOutput();
        if (dmidecodeOutput is null)
            return;

        var memSticks = GetMemorySticks(dmidecodeOutput);
        MemorySticks = memSticks.Select(MemoryStickInfo.FromDictionary);

        // These values are the same for all memory sticks
        var firstStick = memSticks.First();
        Speed = Convert.ToUInt32(firstStick["Speed"].Split(' ')[0]);
        Voltage = (uint)(Convert.ToDouble(firstStick["Configured Voltage"].Split(' ')[0]) * 1000);
        FormFactor = firstStick["Form Factor"];
        Type = firstStick["Type"];

        // Refresh values in case /proc/meminfo is inaccurate
        TotalMemory = MemorySticks
            .Select(ms => ms.Capacity)
            .Aggregate((a, b) => a + b);
        ReservedMemory = TotalMemory - UsableMemory;
    }

    /// <summary>
    /// String representation of the memory form factor
    /// </summary>
    /// <param name="value">Value of type</param>
    /// <remarks>It seems like Windows uses slightly different values than SMBIOS
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory">Microsoft docs</a></remarks>
    [SupportedOSPlatform("windows")]
    private static string? GetMemoryFormFactor(ushort value) =>
        value switch
        {
            2 => "SIP",
            3 => "DIP",
            4 => "ZIP",
            5 => "SOJ",
            7 => "SIMM",
            8 => "DIMM",
            9 => "TSOP",
            10 => "PGA",
            11 => "RIMM",
            12 => "SODIMM",
            13 => "SRIMM",
            14 => "SMD",
            15 => "SSMP",
            16 => "QFP",
            17 => "TQFP",
            18 => "SOIC",
            19 => "LCC",
            20 => "PLCC",
            21 => "BGA",
            22 => "FPBGA",
            23 => "LGA",
            _ => null // Unknown, other or proprietary
        };

    /// <summary>
    /// String representation of values from SMBIOS Memory Device (Type 17) - Type
    /// </summary>
    /// <param name="value">Value of type</param>
    private static string? GetMemoryType(uint value) =>
        value switch
        {
            0x03 => "DRAM",
            0x04 => "EDRAM",
            0x05 => "VRAM",
            0x06 => "SRAM",
            0x07 => "RAM",
            0x08 => "ROM",
            0x09 => "FLASH",
            0x0A => "EEPROM",
            0x0B => "FEPROM",
            0x0C => "EPROM",
            0x0D => "CDRAM",
            0x0E => "3DRAM",
            0x0F => "SDRAM",
            0x10 => "SGRAM",
            0x11 => "RDRAM",
            0x12 => "DDR",
            0x13 => "DDR2",
            0x14 => "DDR2 FB-DIMM",
            0x18 => "DDR3",
            0x19 => "FBD2",
            0x1A => "DDR4",
            0x1B => "LPDDR",
            0x1C => "LPDDR2",
            0x1D => "LPDDR3",
            0x1E => "LPDDR4",
            0x1F => "Logical non-volatile device",
            0x20 => "HBM",
            0x21 => "HBM2",
            0x22 => "DDR5",
            0x23 => "LPDDR5",
            _ => null
        };

    [SupportedOSPlatform("linux")]
    private static List<Dictionary<string, string>> GetMemorySticks(string dmidecodeOutput)
    {
        var lines = dmidecodeOutput.Split(Environment.NewLine);

        var deviceIndices = lines
            .Select((line, index) => (line, index))
            .Where(line => line.line.StartsWith("Memory Device"))
            .Select(line => line.index)
            .ToList();

        var devices = new List<Dictionary<string, string>>();

        for (var i = 0; i < deviceIndices.Count; i++)
        {
            var end = i + 1 < deviceIndices.Count ? deviceIndices[i + 1] : lines.Length;
            var properties = lines[deviceIndices[i]..end]
                .Select(line => line.Split(':'))
                .Where(line => line.Length == 2)
                .ToDictionary(line => line[0].Trim(), line => line[1].Trim());
            devices.Add(properties);
        }

        return devices;
    }

    [SupportedOSPlatform("linux")]
    private static string? GetDmidecodeOutput()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dmidecode",
                    Arguments = "--type memory",
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Permission denied") ? null : output;
        }
        catch
        {
            return null;
        }
    }
}