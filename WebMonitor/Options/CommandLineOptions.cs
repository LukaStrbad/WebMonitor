using CommandLine;

namespace WebMonitor.Options;

public class CommandLineOptions
{
    [Option("ip", Required = false, HelpText = "Listen on specified IP addresses.", Separator = ',')]
    public IEnumerable<string> Ips { get; set; } = Enumerable.Empty<string>();
}