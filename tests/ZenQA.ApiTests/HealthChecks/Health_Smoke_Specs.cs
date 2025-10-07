using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using RestSharp;
using ZenQA.ApiTests.Common;

namespace ZenQA.ApiTests.HealthChecks;

public class Health_Smoke_Specs : TestBase
{
    [Test]
    public async Task base_url_should_be_reachable()
    {
        var resp = await new RequestBuilder().For("/objects").WithMethod(Method.Get).Send(Client);
        ((int)resp.StatusCode).Should().BeInRange(200, 499); // endpoint exists
    }
}