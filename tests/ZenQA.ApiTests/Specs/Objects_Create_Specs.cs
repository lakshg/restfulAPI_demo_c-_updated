using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.Specs;

public class Objects_Create_Specs : TestBase
{
    [Test]
    public async Task create_should_return_id_and_echo_fields()
    {
        var payload = new
        {
            name = "zenqa-create",
            data = new { brand = "BrandX", capacity = "32GB" }
        };

        var resp = await new RequestBuilder().For("/objects")
            .WithMethod(Method.Post).WithJsonBody(payload).Send(Client);

        resp.IsSuccessful.Should().BeTrue();
        var body = JObject.Parse(resp.Content!);
        body["id"]!.ToString().Should().NotBeNullOrWhiteSpace();
        body["name"]!.ToString().Should().Be("zenqa-create");
        body["data"]!["capacity"]!.ToString().Should().Be("32GB");
    }
}