using RestSharp;

namespace ZenQA.ApiTests.Common;

public static class ApiClient
{
    public static RestClient Create()
    {
        var options = new RestClientOptions(TestConfig.BaseUrl)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(30)
        };
        var client = new RestClient(options);

        foreach (var kv in TestConfig.DefaultHeaders)
            client.AddDefaultHeader(kv.Key, kv.Value);

        client.AddDefaultHeader("User-Agent", "ZenQA-ApiTests/1.0");
        client.AddDefaultHeader("Accept", "application/json");
        return client;
    }
}