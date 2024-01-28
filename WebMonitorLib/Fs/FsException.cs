namespace WebMonitorLib.Fs;

public class FsException(string message, Exception innerException) : Exception(message, innerException);