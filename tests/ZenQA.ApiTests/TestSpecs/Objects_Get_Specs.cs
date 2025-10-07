using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.TestSpecs;

public class Objects_Get_Specs : TestBase
{
    [Test]
    public async Task get_single_object_should_return_200_or_404()
    {
        // Request specific object by ID
        var resp = await new RequestBuilder().For("/objects/1").WithMethod(Method.Get).Send(Client);
        
        // Object may exist (200) or not exist (404) - both are valid responses
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 404 });
    }

    [Test]
    public async Task get_list_should_return_array()
    {
        // Request list of all objects
        var resp = await new RequestBuilder().For("/objects").WithMethod(Method.Get).Send(Client);
        
        // Debug: Print response details
        Console.WriteLine($"Status Code: {resp.StatusCode}");
        Console.WriteLine($"Status Description: {resp.StatusDescription}");
        Console.WriteLine($"Response Content: {resp.Content}");
        Console.WriteLine($"Error Message: {resp.ErrorMessage}");
        Console.WriteLine($"Response Headers: {string.Join(", ", resp.Headers?.Select(h => $"{h.Name}:{h.Value}") ?? new string[0])}");
        
        // Should always return success with array response
        resp.IsSuccessful.Should().BeTrue();
        var arr = JArray.Parse(resp.Content!);
        arr.Should().NotBeNull(); // Verify response is a valid JSON array
    }
}
