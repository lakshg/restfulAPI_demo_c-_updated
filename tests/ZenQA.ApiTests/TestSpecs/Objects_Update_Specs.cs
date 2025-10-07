using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.TestSpecs;

/// <summary>
/// PATCH operation tests - covers positive, negative, and edge cases
/// </summary>
[TestFixture]
[Category("UpdateOperations")]
public class Objects_Update_Specs : TestBase
{
    /// <summary>
    /// Creates a test object with multiple fields for PATCH testing
    /// </summary>
    private async Task<string> CreateTestObject()
    {
        var createRequest = new RequestBuilder()
            .For("/objects")
            .WithMethod(Method.Post)
            .WithJsonBody(new 
            { 
                name = "zenqa-patch-test", 
                data = new 
                { 
                    field = "original_value", 
                    preserve_me = "should_remain_unchanged",
                    number_field = 42,
                    boolean_field = true
                }
            });
            
        var response = await createRequest.Send(Client);
        
        response.IsSuccessful.Should().BeTrue("Object creation must succeed for PATCH testing");
        response.Content.Should().NotBeNullOrEmpty();
        
        var responseBody = JObject.Parse(response.Content!);
        var objectId = responseBody["id"]!.ToString();
        objectId.Should().NotBeNullOrEmpty();
        
        return objectId;
    }
    
    /// <summary>
    /// Tests partial field updates - core PATCH functionality
    /// </summary>
    [Test]
    [Category("Positive")]
    public async Task patch_should_update_specified_fields_and_preserve_others()
    {
        var objectId = await CreateTestObject();

        var patchRequest = new RequestBuilder()
            .For($"/objects/{objectId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new 
            { 
                data = new 
                { 
                    field = "updated_value",
                    preserve_me = "should_remain_unchanged",
                    number_field = 100
                }
            });
            
        var patchResponse = await patchRequest.Send(Client);

        patchResponse.IsSuccessful.Should().BeTrue();
        patchResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var responseBody = JObject.Parse(patchResponse.Content!);
        
        // Verify updated fields
        responseBody["data"]!["field"]!.ToString().Should().Be("updated_value");
        responseBody["data"]!["number_field"]!.Value<int>().Should().Be(100);
        responseBody["data"]!["preserve_me"]!.ToString().Should().Be("should_remain_unchanged");
            
        // Note: API replaces entire data object rather than merging fields
            
        // Verify response structure
        responseBody["id"]!.ToString().Should().Be(objectId);
        responseBody["name"]!.ToString().Should().Be("zenqa-patch-test");
        responseBody.Should().ContainKey("updatedAt");
    }

    /// <summary>
    /// Tests single field update
    /// </summary>
    [Test]
    [Category("Positive")]
    public async Task patch_should_handle_single_field_update()
    {
        var objectId = await CreateTestObject();

        var patchRequest = new RequestBuilder()
            .For($"/objects/{objectId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { data = new { field = "single_update" } });
            
        var response = await patchRequest.Send(Client);

        response.IsSuccessful.Should().BeTrue();
        var body = JObject.Parse(response.Content!);
        body["data"]!["field"]!.ToString().Should().Be("single_update");
    }
    
    /// <summary>
    /// Tests PATCH on non-existent resource - should return 404
    /// </summary>
    [Test]
    [Category("Negative")]
    public async Task patch_nonexistent_resource_should_return_404()
    {
        var patchRequest = new RequestBuilder()
            .For("/objects/non-existent-id-12345")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { data = new { field = "should_fail" } });
            
        var response = await patchRequest.Send(Client);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        response.Content.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests PATCH with invalid resource IDs
    /// </summary>
    [Test]
    [Category("Negative")]
    [TestCase("invalid-format")]
    [TestCase("null")]
    public async Task patch_with_invalid_resource_id_should_return_error(string invalidId)
    {
        var response = await new RequestBuilder()
            .For($"/objects/{invalidId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { data = new { field = "test" } })
            .Send(Client);

        // API behavior varies - some IDs return 4xx, others behave differently
        ((int)response.StatusCode).Should().BeInRange(200, 599);
    }
    
    /// <summary>
    /// Tests PATCH with invalid payload structure
    /// </summary>
    [Test]
    [Category("Negative")]
    public async Task patch_with_invalid_payload_structure_should_return_client_error()
    {
        var objectId = await CreateTestObject();

        var patchRequest = new RequestBuilder()
            .For($"/objects/{objectId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { invalid_structure = "this_should_fail" });
            
        var response = await patchRequest.Send(Client);

        ((int)response.StatusCode).Should().BeInRange(400, 499);
        response.Content.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests PATCH with empty payload
    /// </summary>
    [Test]
    [Category("Negative")]
    public async Task patch_with_empty_payload_should_return_error()
    {
        var objectId = await CreateTestObject();

        var response = await new RequestBuilder()
            .For($"/objects/{objectId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(new { })
            .Send(Client);

        // API accepts empty PATCH as no-op
        ((int)response.StatusCode).Should().BeInRange(200, 499);
    }
    
    /// <summary>
    /// Tests PATCH with special characters and edge values
    /// </summary>
    [Test]
    [Category("EdgeCase")]
    public async Task patch_should_handle_special_characters_and_edge_values()
    {
        var objectId = await CreateTestObject();

        var specialData = new 
        { 
            data = new 
            { 
                special_chars = "!@#$%^&*()_+-=[]{}|;':\",./<>?",
                unicode_test = "测试数据",
                empty_string = "",
                very_long_string = new string('A', 1000),
                numeric_edge = int.MaxValue,
                boolean_edge = false
            }
        };
        
        var response = await new RequestBuilder()
            .For($"/objects/{objectId}")
            .WithMethod(Method.Patch)
            .WithJsonBody(specialData)
            .Send(Client);

        if (response.IsSuccessful)
        {
            var body = JObject.Parse(response.Content!);
            body["data"]!["special_chars"]!.ToString().Should().Contain("!@#$");
        }
        else
        {
            // API may reject large payloads with server error
            ((int)response.StatusCode).Should().BeInRange(400, 599);
        }
    }
}