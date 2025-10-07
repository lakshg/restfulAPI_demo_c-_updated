using RestSharp;

namespace ZenQA.ApiTests.Common;

/// <summary>
// Creates configured HTTP clients for API testing
// </summary>
public static class ApiClient
{
    /// <summary>
    // Creates a RestClient with standard test configuration
    // </summary>
    public static RestClient Create()
    {
        var options = new RestClientOptions(TestConfig.BaseUrl)
        {
            ThrowOnAnyError = false,  // Handle errors manually in tests
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        var client = new RestClient(options);

        // Add configured headers
        foreach (var kv in TestConfig.DefaultHeaders)
            client.AddDefaultHeader(kv.Key, kv.Value);

        // Add standard test headers
        client.AddDefaultHeader("User-Agent", "ZenQA-ApiTests/1.0");
        client.AddDefaultHeader("Accept", "application/json");
        
        return client;
    }
}