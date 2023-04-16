namespace WebMonitor.Utility;

public static class NumberUtility
{
    public static long MbToB(float value) => (long)(value * 1024ul * 1024ul);
}