using NUnit.Framework;
using RestSharp;
using Serilog;

namespace ZenQA.ApiTests.Common;

// Base class for all API test classes with logging and reporting setup
public abstract class TestBase
{
    protected RestClient Client = null!;
    private string _currentTestName = "";

    // One-time setup for entire test run
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Configure structured logging to file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("tests/ZenQA.ApiTests/reports/test-execution.log", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Initialize test reporter
        TestReporter.ClearResults();
    }

    // Setup before each test
    [SetUp]
    public void SetUp()
    {
        _currentTestName = TestContext.CurrentContext.Test.Name;
        Client = ApiClient.Create(); // Create fresh HTTP client for each test
        
        // Extract test description from attributes
        var testMethod = GetType().GetMethod(_currentTestName);
        var description = testMethod?.GetCustomAttributes(typeof(TestAttribute), false)
            .Cast<TestAttribute>()
            .FirstOrDefault()?.Description ?? "";
            
        // Log test start for reporting
        TestReporter.LogTestStart(_currentTestName, description);
    }

    // Cleanup after each test
    [TearDown]
    public void TearDown()
    {
        var context = TestContext.CurrentContext;
        
        // Convert NUnit status to internal enum
        var status = context.Result.Outcome.Status switch
        {
            NUnit.Framework.Interfaces.TestStatus.Passed => TestStatus.Passed,
            NUnit.Framework.Interfaces.TestStatus.Failed => TestStatus.Failed,
            NUnit.Framework.Interfaces.TestStatus.Skipped => TestStatus.Skipped,
            _ => TestStatus.Failed
        };

        // Log test completion with result details
        TestReporter.LogTestEnd(_currentTestName, status, 
            context.Result.Message, 
            context.Result.StackTrace);
    }

    // Final cleanup after all tests complete
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Generate detailed HTML report
        var reportPath = Path.Combine("tests", "ZenQA.ApiTests", "reports", "detailed-test-report.html");
        await TestReporter.GenerateDetailedHtmlReport(reportPath);
        
        // Close logging system
        Log.CloseAndFlush();
    }
}