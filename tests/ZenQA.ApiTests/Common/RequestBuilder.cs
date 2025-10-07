using RestSharp;
using System.Diagnostics;

namespace ZenQA.ApiTests.Common;

// Fluent builder for creating HTTP requests
public class RequestBuilder
{
    private string _resource = "/";
    private Method _method = Method.Get;
    private readonly Dictionary<string,string> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string,string> _query = new(StringComparer.OrdinalIgnoreCase);
    private object? _jsonBody;

    // Set the API endpoint path
    public RequestBuilder For(string resource)
    {
        _resource = resource.StartsWith("/") ? resource : "/" + resource; // Ensure leading slash
        return this;
    }

    // Set HTTP method (GET, POST, PUT, etc.)
    public RequestBuilder WithMethod(Method method) { _method = method; return this; }

    // Add custom header
    public RequestBuilder WithHeader(string key, string value) { _headers[key] = value; return this; }

    // Add query parameter
    public RequestBuilder WithQuery(string key, string value) { _query[key] = value; return this; }

    // Set JSON request body
    public RequestBuilder WithJsonBody(object body) { _jsonBody = body; return this; }

    // Build the final RestRequest object
    public RestRequest Build()
    {
        var req = new RestRequest(_resource, _method);

        // Add all headers and query parameters
        foreach (var kv in _headers) req.AddOrUpdateHeader(kv.Key, kv.Value);
        foreach (var kv in _query) req.AddOrUpdateParameter(kv.Key, kv.Value);

        // Add JSON body if provided
        if (_jsonBody is not null) req.AddJsonBody(_jsonBody);
        return req;
    }

    // Execute the request and capture timing/logging
    public async Task<RestResponse> Send(RestClient client)
    {
        var request = Build();
        var stopwatch = Stopwatch.StartNew();
        
        // Log the request details
        TestReporter.LogRequest(request, _resource);
        
        // Execute the HTTP request
        var response = await client.ExecuteAsync(request);
        
        stopwatch.Stop();
        
        // Log the response details with timing
        TestReporter.LogResponse(response, stopwatch.Elapsed);
        
        return response;
    }
}