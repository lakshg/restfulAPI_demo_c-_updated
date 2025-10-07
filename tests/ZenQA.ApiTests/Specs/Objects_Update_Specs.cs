using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.Specs;

public class Objects_Update_Specs : TestBase
{
    private async Task<string> CreateObject()
    {
        var create = await new RequestBuilder().For("/objects").WithMethod(Method.Post)
            .WithJsonBody(new { name = "zenqa-patch", data = new { field = "orig", keep = "ok" }})
            .Send(Client);
        create.IsSuccessful.Should().BeTrue();
        var id = JObject.Parse(create.Content!)["id"]!.ToString();
        return id;
    }

    [Test]
    public async Task patch_should_update_partial_fields_and_keep_others()
    {
        var id = await CreateObject();

        var patch = await new RequestBuilder().For($"/objects/{id}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { data = new { field = "patched", keep = "ok" } })
            .Send(Client);

        patch.IsSuccessful.Should().BeTrue();
        var body = JObject.Parse(patch.Content!);
        body["data"]!["field"]!.ToString().Should().Be("patched");
        body["data"]!["keep"]!.ToString().Should().Be("ok");
    }

    [Test]
    public async Task patch_nonexistent_should_be_404()
    {
        var resp = await new RequestBuilder().For("/objects/does-not-exist")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { data = new { field = "x" } })
            .Send(Client);
        ((int)resp.StatusCode).Should().Be(404);
    }

    [Test]
    public async Task patch_with_invalid_payload_should_be_4xx()
    {
        var id = await CreateObject();
        var resp = await new RequestBuilder().For($"/objects/{id}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { nonsense = "bad" })
            .Send(Client);

        ((int)resp.StatusCode).Should().BeInRange(400,499);
    }
}