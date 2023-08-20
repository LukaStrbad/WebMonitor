using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using Windows.Win32;
using Windows.Win32.System.Power;
using LibreHardwareMonitor.Hardware;

namespace WebMonitor.Native.Cpu;

public partial class CpuInfo
{
    /// <summary>
    /// Name of the CPU
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>
    /// CPU identifier
    /// </summary>
    [JsonPropertyName("identifier")]
    public string Identifier { get; }

    /// <summary>
    /// Number of threads (logical cores)
    /// </summary>
    [JsonPropertyName("numThreads")]
    public int NumThreads { get; }

    /// <summary>
    /// Number of physical cores
    /// </summary>
    [JsonPropertyName("numCores")]
    public int NumCores { get; }

    /// <summary>
    /// A list of base frequencies of each thread
    /// </summary>
    [JsonPropertyName("baseFrequencies")]
    public List<uint> BaseFrequencies { get; }
    
    public CpuInfo(IComputer computer)
    {
        // CPU is always there so we don't need to use FirstOrDefault
        var cpu = computer.Hardware.First(h => h.HardwareType == HardwareType.Cpu);

        Name = cpu.Name;
        Identifier = cpu.Identifier.ToString();
        NumThreads = Environment.ProcessorCount;
        // Only physical cores have a clock LibreHardwareMonitor
        // TODO: Investigate why it returned 9 on my 8 core CPU and possible fetch data from the OS instead
        NumCores = cpu.Sensors.Count(s => s.SensorType == SensorType.Clock);

        BaseFrequencies = new();

        // We can't use LibreHardwareMonitor for frequencies on Windows because it requires admin privileges
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            using var ppInfo = new DisposablePointer<PROCESSOR_POWER_INFORMATION>(Environment.ProcessorCount);

            unsafe
            {
                var res = PInvoke.CallNtPowerInformation(
                    POWER_INFORMATION_LEVEL.ProcessorInformation,
                    (void*)0,
                    0,
                    (void*)ppInfo,
                    (uint)ppInfo.Size
                );
            }

            BaseFrequencies.AddRange(ppInfo.Select(info => info.MaxMhz));
        }
        else if (OperatingSystem.IsLinux())
        {
            // Base directory that contains infos about each thread
            var baseCpuDir = new DirectoryInfo("/sys/devices/system/cpu");
            var cpuRegex = CpuRegex();
            foreach (var cpuDir in baseCpuDir.GetDirectories().Where(dir => cpuRegex.IsMatch(dir.Name)))
            {
                // File that contains the max frequency without boost
                var maxFreqFile = Path.Combine(cpuDir.FullName, "cpufreq/scaling_max_freq");

                if (!File.Exists(maxFreqFile))
                    continue;

                if (uint.TryParse(File.ReadAllText(maxFreqFile), out var freq))
                {
                    // We need to convert the value to MHz
                    BaseFrequencies.Add(freq / 1000);
                }
            }
        }
    }

    [GeneratedRegex("cpu\\d+")]
    private static partial Regex CpuRegex();
}
