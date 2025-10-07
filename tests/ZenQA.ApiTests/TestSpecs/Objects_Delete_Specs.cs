using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.TestSpecs;

public class Objects_Delete_Specs : TestBase
{
    // Helper method to create a test object before deletion
    private async Task<string> CreateObject()
    {
        var create = await new RequestBuilder().For("/objects").WithMethod(Method.Post)
            .WithJsonBody(new { name = "zenqa-delete", data = new { a = 1 }})
            .Send(Client);
        create.IsSuccessful.Should().BeTrue();
        return JObject.Parse(create.Content!)["id"]!.ToString(); // Return the created object ID
    }

    [Test]
    public async Task delete_should_return_200_or_204()
    {
        // Create object first
        var id = await CreateObject();
        
        // Delete the object
        var resp = await new RequestBuilder().For($"/objects/{id}")
            .WithMethod(Method.Delete).Send(Client);
            
        // Verify delete returns success status (200 OK or 204 No Content)
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 204 });
    }
}
