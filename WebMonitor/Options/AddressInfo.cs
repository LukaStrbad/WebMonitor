using System.Text.RegularExpressions;

namespace WebMonitor.Options;

public partial record AddressInfo(string Ip, int Port, bool UseHttps = false)
{
    public static IEnumerable<AddressInfo> ParseFromStrings(IEnumerable<string> ips)
    {
        foreach (var ip in ips)
        {
            var regex = IpRegex();
            var match = regex.Match(ip);
            var protocolGroup = match.Groups["protocol"];
            var ipGroup = match.Groups["ip"];
            var portGroup = match.Groups["port"];

            if (!ipGroup.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"""Invalid ip address: "{ip}" """);
                Console.ResetColor();
                Environment.Exit(1);
            }

            var ipAddress = ipGroup.Value;
            var useHttps = protocolGroup.Value == "https";
            // Parse won't fail because regex ensures that the port is a number.
            var port = portGroup.Success ? int.Parse(portGroup.Value) : 5000;

            yield return new AddressInfo(ipAddress, port, useHttps);
        }
    }

    [GeneratedRegex("((?<protocol>https?)://)?(?<ip>(\\d{1,3}\\.){3}\\d{1,3})(:(?<port>\\d{1,5}))?")]
    private static partial Regex IpRegex();
}