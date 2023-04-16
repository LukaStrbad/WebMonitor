using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using Windows.Win32;
using Windows.Win32.System.WindowsProgramming;
using WebMonitor.Native.Cpu.Win32;

namespace WebMonitor.Native.Cpu;

public class CpuUsage : IRefreshable
{
    /// <summary>
    /// A list of CPU usages for each thread in %
    /// </summary>
    /// <returns></returns>
    [JsonPropertyName("threadUsages")]
    public List<float> ThreadUsages { get; } = new();

    private readonly long[] _oldIdleTimes = new long[Environment.ProcessorCount];
    private readonly long[] _oldKernelTimes = new long[Environment.ProcessorCount];
    private readonly long[] _oldUserTimes = new long[Environment.ProcessorCount];

    [SupportedOSPlatform("windows10.0")]
    private void RefreshWindows(int millisSinceRefresh)
    {
        unsafe
        {
            // Windows 11 22H2 has wrong CPU idle times
            // More info: https://forums.guru3d.com/threads/msi-ab-rtss-development-news-thread.412822/page-179
            using var perfInfo =
                new DisposablePointer<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(Environment.ProcessorCount);
            using var idleInfo = new DisposablePointer<SYSTEM_PROCESSOR_IDLE_INFORMATION>(Environment.ProcessorCount);

            // Get kernel and user times
            var retLen = 0u;
            var res = PInvoke.NtQuerySystemInformation(
                SYSTEM_INFORMATION_CLASS.SystemProcessorPerformanceInformation,
                (void*)perfInfo,
                SystemInformationLength: (uint)perfInfo.Size,
                ref retLen
            );
            if (res != 0)
                throw new Exception($"{nameof(PInvoke.NtQuerySystemInformation)} call wasn't successfull");

            // Get idle times with a separate call because SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION returns wrong times
            res = PInvoke.NtQuerySystemInformation(
                (SYSTEM_INFORMATION_CLASS)0x002A, // Undocumented constant
                (void*)idleInfo,
                (uint)idleInfo.Size,
                ref retLen
            );
            if (res != 0)
                throw new Exception($"{nameof(PInvoke.NtQuerySystemInformation)} call wasn't successfull");

            // Lock prevents other threads from accessing empty list
            lock (ThreadUsages)
            {
                ThreadUsages.Clear();

                for (var i = 0; i < Environment.ProcessorCount; i++)
                {
                    var idleTime = (long)idleInfo[i].IdleTime;
                    var kernelTime = perfInfo[i].KernelTime;
                    var userTime = perfInfo[i].UserTime;

                    var idl = idleTime - _oldIdleTimes[i];
                    var ker = kernelTime - _oldKernelTimes[i];
                    var usr = userTime - _oldUserTimes[i];
                    var sys = ker + usr;
                    ThreadUsages.Add((float)((sys - idl) * 100.0 / sys));

                    _oldIdleTimes[i] = idleTime;
                    _oldKernelTimes[i] = kernelTime;
                    _oldUserTimes[i] = userTime;
                }
            }
        }
    }

    [SupportedOSPlatform("linux")]
    private void RefreshLinux(int millisSinceRefresh)
    {
        var stats = File
            .ReadAllText("/proc/stat")
            .Split(Environment.NewLine)[1..(Environment.ProcessorCount + 1)]; // Discard total values

        lock (ThreadUsages)
        {
            ThreadUsages.Clear();

            // Indexed foreach
            foreach (var (line, i) in stats.Select((line, i) => (line, i)))
            {
                // Split spaces and discard cpu name
                var split = line.Split(' ')[1..];
                var idleTime = Convert.ToInt64(split[3]);
                var totalTime = split.Select(val => Convert.ToInt64(val)).Sum();

                var idl = idleTime - _oldIdleTimes[i];
                var tot = totalTime - _oldUserTimes[i]; // Reuse user times for total
                ThreadUsages.Add(float.Clamp((1f - (float)idl / tot) * 100f, 0, 100));

                _oldIdleTimes[i] = idleTime;
                _oldUserTimes[i] = totalTime;
            }
        }
    }

    public void Refresh(int millisSinceRefresh)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
            RefreshWindows(millisSinceRefresh);
        else if (OperatingSystem.IsLinux())
            RefreshLinux(millisSinceRefresh);
    }
}