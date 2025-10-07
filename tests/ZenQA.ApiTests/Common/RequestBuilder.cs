using RestSharp;
using System.Diagnostics;

namespace ZenQA.ApiTests.Common;

public class RequestBuilder
{
    private string _resource = "/";
    private Method _method = Method.Get;
    private readonly Dictionary<string,string> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string,string> _query = new(StringComparer.OrdinalIgnoreCase);
    private object? _jsonBody;

    public RequestBuilder For(string resource)
    {
        _resource = resource.StartsWith("/") ? resource : "/" + resource;
        return this;
    }

    public RequestBuilder WithMethod(Method method) { _method = method; return this; }

    public RequestBuilder WithHeader(string key, string value) { _headers[key] = value; return this; }

    public RequestBuilder WithQuery(string key, string value) { _query[key] = value; return this; }

    public RequestBuilder WithJsonBody(object body) { _jsonBody = body; return this; }

    public RestRequest Build()
    {
        var req = new RestRequest(_resource, _method);

        foreach (var kv in _headers) req.AddOrUpdateHeader(kv.Key, kv.Value);
        foreach (var kv in _query) req.AddOrUpdateParameter(kv.Key, kv.Value);

        if (_jsonBody is not null) req.AddJsonBody(_jsonBody);
        return req;
    }

    public async Task<RestResponse> Send(RestClient client)
    {
        var request = Build();
        var stopwatch = Stopwatch.StartNew();
        
        // Log the request
        TestReporter.LogRequest(request, _resource);
        
        // Execute the request
        var response = await client.ExecuteAsync(request);
        
        stopwatch.Stop();
        
        // Log the response
        TestReporter.LogResponse(response, stopwatch.Elapsed);
        
        return response;
    }
}