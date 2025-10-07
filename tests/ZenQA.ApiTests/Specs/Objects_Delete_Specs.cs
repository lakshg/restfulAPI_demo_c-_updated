using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.Specs;

public class Objects_Delete_Specs : TestBase
{
    private async Task<string> CreateObject()
    {
        var create = await new RequestBuilder().For("/objects").WithMethod(Method.Post)
            .WithJsonBody(new { name = "zenqa-delete", data = new { a = 1 }})
            .Send(Client);
        create.IsSuccessful.Should().BeTrue();
        return JObject.Parse(create.Content!)["id"]!.ToString();
    }

    [Test]
    public async Task delete_should_return_200_or_204()
    {
        var id = await CreateObject();
        var resp = await new RequestBuilder().For($"/objects/{id}")
            .WithMethod(Method.Delete).Send(Client);
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 204 });
    }
}