using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WebMonitor.Native.Cpu.Win;

/// <summary>
/// Undocumented Win32 struct so we can't use CsWin32 to generate it
/// </summary>
[SupportedOSPlatform("windows10.0")]
[StructLayout(LayoutKind.Sequential)]
internal struct SYSTEM_PROCESSOR_IDLE_INFORMATION
{
	public ulong IdleTime;
	public ulong C1Time;
	public ulong C2Time;
	public ulong C3Time;
	public uint C1Transitions;
	public uint C2Transitions;
	public uint C3Transitions;
	private readonly uint _padding;

	public SYSTEM_PROCESSOR_IDLE_INFORMATION()
	{
		IdleTime = 0;
		C1Time = 0;
		C2Time = 0;
		C3Time = 0;
		C1Transitions = 0;
		C2Transitions = 0;
		C3Transitions = 0;
		_padding = 0;
	}
}
