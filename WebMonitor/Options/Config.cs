using System.Security;
using System.Text.Json;

namespace WebMonitor.Options;

public class Config
{
    /// <summary>
    /// List of addresses to listen on
    /// </summary>
    public List<AddressInfo> Addresses { get; set; } = new();

    public const string ConfigPath = "config.json";

    private static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#endif
            return false;
        }
    }

    public static Config Load(bool loadFromFile = true)
    {
        // Don't load from file if in debug mode
        if (IsDebug || !loadFromFile || !File.Exists(ConfigPath))
            return new Config();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return config ?? new Config();
        }
        catch (Exception e)
            when (e is UnauthorizedAccessException or IOException or SecurityException)
        {
            Console.WriteLine("Error opening config file.");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error parsing config file: {e.Message}");
        }
        catch
        {
            Console.WriteLine("Unknown error while parsing config file.");
        }

        Console.WriteLine("Using default config.");

        return new Config();
    }
}