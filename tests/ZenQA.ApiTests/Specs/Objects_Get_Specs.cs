using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;
using Newtonsoft.Json.Linq;

namespace ZenQA.ApiTests.Specs;

public class Objects_Get_Specs : TestBase
{
    [Test]
    public async Task get_single_object_should_return_200_or_404()
    {
        var resp = await new RequestBuilder().For("/objects/1").WithMethod(Method.Get).Send(Client);
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 404 });
    }

    [Test]
    public async Task get_list_should_return_array()
    {
        var resp = await new RequestBuilder().For("/objects").WithMethod(Method.Get).Send(Client);
        resp.IsSuccessful.Should().BeTrue();
        var arr = JArray.Parse(resp.Content!);
        arr.Should().NotBeNull();
    }
}