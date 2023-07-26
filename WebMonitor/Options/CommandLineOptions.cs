using CommandLine;

namespace WebMonitor.Options;

public class CommandLineOptions
{
    [Option("ip", Required = false, HelpText = "Listen on specified IP addresses.", Separator = ',')]
    public IEnumerable<string> Ips { get; set; } = Enumerable.Empty<string>();

    [Option("promote-to-admin", Required = false, HelpText = "Promote the specified user to admin.")]
    public string? PromoteToAdmin { get; set; }
}
