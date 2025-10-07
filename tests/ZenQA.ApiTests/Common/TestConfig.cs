using System.Text.Json.Nodes;

namespace ZenQA.ApiTests.Common;

// Configuration manager for test environment settings
public static class TestConfig
{
    private static readonly JsonObject Root;

    // Load configuration from appsettings.json at startup
    static TestConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);
        Root = JsonNode.Parse(json)!.AsObject();
    }

    // Get current test environment (from env var or config, defaults to DEV)
    public static string Env =>
        Environment.GetEnvironmentVariable("TEST_ENV")?.Trim().ToUpperInvariant() ??
        (Root["environment"]?.ToString() ?? "DEV").ToUpperInvariant();

    // Get API base URL for current environment
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("API_BASE_URL")?.Trim() ??
        Root[Env]?["baseUrl"]?.ToString() ??
        throw new InvalidOperationException("BaseUrl not configured");

    // Get default headers for current environment
    public static IReadOnlyDictionary<string,string> DefaultHeaders =>
        Root[Env]?["defaultHeaders"]?.AsObject()?.ToDictionary(kv => kv.Key, kv => kv.Value!.ToString())
        ?? new Dictionary<string,string>();

    // Combine default headers with additional headers
    public static IDictionary<string,string> MergedHeaders(IDictionary<string,string>? extra = null)
    {
        var dict = new Dictionary<string,string>(DefaultHeaders, StringComparer.OrdinalIgnoreCase);
        dict["User-Agent"] = "ZenQA-ApiTests/1.0"; // Always add test user agent
        if (extra is not null) foreach (var kv in extra) dict[kv.Key] = kv.Value; // Add extra headers
        return dict;
    }
}