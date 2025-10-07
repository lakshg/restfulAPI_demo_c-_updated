using System.Text.Json.Nodes;

namespace ZenQA.ApiTests.Common;

public static class TestConfig
{
    private static readonly JsonObject Root;

    static TestConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);
        Root = JsonNode.Parse(json)!.AsObject();
    }

    public static string Env =>
        Environment.GetEnvironmentVariable("TEST_ENV")?.Trim().ToUpperInvariant() ??
        (Root["environment"]?.ToString() ?? "DEV").ToUpperInvariant();

    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("API_BASE_URL")?.Trim() ??
        Root[Env]?["baseUrl"]?.ToString() ??
        throw new InvalidOperationException("BaseUrl not configured");

    public static IReadOnlyDictionary<string,string> DefaultHeaders =>
        Root[Env]?["defaultHeaders"]?.AsObject()?.ToDictionary(kv => kv.Key, kv => kv.Value!.ToString())
        ?? new Dictionary<string,string>();

    public static IDictionary<string,string> MergedHeaders(IDictionary<string,string>? extra = null)
    {
        var dict = new Dictionary<string,string>(DefaultHeaders, StringComparer.OrdinalIgnoreCase);
        dict["User-Agent"] = "ZenQA-ApiTests/1.0";
        if (extra is not null) foreach (var kv in extra) dict[kv.Key] = kv.Value;
        return dict;
    }
}