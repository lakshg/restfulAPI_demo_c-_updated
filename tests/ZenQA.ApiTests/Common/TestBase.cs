using NUnit.Framework;
using RestSharp;
using Serilog;

namespace ZenQA.ApiTests.Common;

public abstract class TestBase
{
    protected RestClient Client = null!;
    private string _currentTestName = "";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Initialize Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("tests/ZenQA.ApiTests/reports/test-execution.log", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        TestReporter.ClearResults();
    }

    [SetUp]
    public void SetUp()
    {
        _currentTestName = TestContext.CurrentContext.Test.Name;
        Client = ApiClient.Create();
        
        // Get test description from method attributes if available
        var testMethod = GetType().GetMethod(_currentTestName);
        var description = testMethod?.GetCustomAttributes(typeof(TestAttribute), false)
            .Cast<TestAttribute>()
            .FirstOrDefault()?.Description ?? "";
            
        TestReporter.LogTestStart(_currentTestName, description);
    }

    [TearDown]
    public void TearDown()
    {
        var context = TestContext.CurrentContext;
        var status = context.Result.Outcome.Status switch
        {
            NUnit.Framework.Interfaces.TestStatus.Passed => TestStatus.Passed,
            NUnit.Framework.Interfaces.TestStatus.Failed => TestStatus.Failed,
            NUnit.Framework.Interfaces.TestStatus.Skipped => TestStatus.Skipped,
            _ => TestStatus.Failed
        };

        TestReporter.LogTestEnd(_currentTestName, status, 
            context.Result.Message, 
            context.Result.StackTrace);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        var reportPath = Path.Combine("tests", "ZenQA.ApiTests", "reports", "detailed-test-report.html");
        await TestReporter.GenerateDetailedHtmlReport(reportPath);
        Log.CloseAndFlush();
    }
}