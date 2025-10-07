using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.TestSpecs;

public class Objects_Create_Specs : TestBase
{
    [Test]
    public async Task create_should_return_id_and_echo_fields()
    {
        // Create test object payload
        var payload = new
        {
            name = "zenqa-create",
            data = new { brand = "BrandX", capacity = "32GB" }
        };

        // Send POST request to create object
        var resp = await new RequestBuilder().For("/objects")
            .WithMethod(Method.Post).WithJsonBody(payload).Send(Client);

        // Verify successful creation and response data
        resp.IsSuccessful.Should().BeTrue();
        var body = JObject.Parse(resp.Content!);
        body["id"]!.ToString().Should().NotBeNullOrWhiteSpace(); // Check ID was generated
        body["name"]!.ToString().Should().Be("zenqa-create");   // Check name echoed back
        body["data"]!["capacity"]!.ToString().Should().Be("32GB"); // Check nested data preserved
    }
}
