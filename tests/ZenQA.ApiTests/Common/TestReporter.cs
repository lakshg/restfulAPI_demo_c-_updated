using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace ZenQA.ApiTests.Common;

public static class TestReporter
{
    private static readonly List<TestExecutionDetails> TestResults = new();
    private static readonly ILogger Logger = Log.ForContext(typeof(TestReporter));

    public static void LogTestStart(string testName, string description = "")
    {
        var testDetail = new TestExecutionDetails
        {
            TestName = testName,
            Description = description,
            StartTime = DateTime.UtcNow,
            Status = TestStatus.Running
        };
        
        TestResults.Add(testDetail);
        Logger.Information("🚀 Test Started: {TestName} - {Description}", testName, description);
    }

    public static void LogRequest(RestRequest request, string endpoint)
    {
        var currentTest = GetCurrentTest();
        if (currentTest != null)
        {
            var requestLog = new RequestLogDetails
            {
                Method = request.Method.ToString(),
                Endpoint = endpoint,
                Headers = request.Parameters
                    .Where(p => p.Type == ParameterType.HttpHeader)
                    .ToDictionary(p => p.Name!, p => p.Value?.ToString() ?? ""),
                Body = GetRequestBody(request),
                Timestamp = DateTime.UtcNow
            };
            
            currentTest.HttpRequests.Add(requestLog);
            Logger.Information("📤 HTTP {Method} {Endpoint}", request.Method, endpoint);
        }
    }

    public static void LogResponse(RestResponse response, TimeSpan duration)
    {
        var currentTest = GetCurrentTest();
        if (currentTest?.HttpRequests.LastOrDefault() is RequestLogDetails lastRequest)
        {
            lastRequest.Response = new ResponseLogDetails
            {
                StatusCode = (int)response.StatusCode,
                StatusDescription = response.StatusDescription ?? "",
                Headers = GetResponseHeaders(response),
                Body = response.Content ?? "",
                ContentType = response.ContentType ?? "",
                Duration = duration,
                Timestamp = DateTime.UtcNow
            };

            Logger.Information("📥 HTTP {StatusCode} {StatusDescription} - {Duration}ms", 
                response.StatusCode, response.StatusDescription, duration.TotalMilliseconds);
        }
    }

    public static void LogTestEnd(string testName, TestStatus status, string? errorMessage = null, string? stackTrace = null)
    {
        var test = TestResults.FirstOrDefault(t => t.TestName == testName && t.Status == TestStatus.Running);
        if (test != null)
        {
            test.EndTime = DateTime.UtcNow;
            test.Status = status;
            test.ErrorMessage = errorMessage;
            test.StackTrace = stackTrace;
            test.Duration = test.EndTime - test.StartTime;

            var statusIcon = status switch
            {
                TestStatus.Passed => "✅",
                TestStatus.Failed => "❌",
                TestStatus.Skipped => "⏭️",
                _ => "⚠️"
            };

            Logger.Information("{StatusIcon} Test {Status}: {TestName} - {Duration}ms", 
                statusIcon, status, testName, test.Duration.TotalMilliseconds);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Logger.Error("💥 Test Failed: {TestName} - {ErrorMessage}", testName, errorMessage);
            }
        }
    }

    public static async Task GenerateDetailedHtmlReport(string outputPath)
    {
        var reportData = new
        {
            GeneratedAt = DateTime.UtcNow,
            Summary = new
            {
                TotalTests = TestResults.Count,
                Passed = TestResults.Count(t => t.Status == TestStatus.Passed),
                Failed = TestResults.Count(t => t.Status == TestStatus.Failed),
                Skipped = TestResults.Count(t => t.Status == TestStatus.Skipped),
                TotalDuration = TestResults.Sum(t => t.Duration.TotalMilliseconds),
                PassRate = TestResults.Count > 0 ? (double)TestResults.Count(t => t.Status == TestStatus.Passed) / TestResults.Count * 100 : 0
            },
            Tests = TestResults
        };

        var html = GenerateHtmlReport(reportData);
        await File.WriteAllTextAsync(outputPath, html);
        Logger.Information("📊 Detailed HTML report generated: {OutputPath}", outputPath);
    }

    private static TestExecutionDetails? GetCurrentTest()
    {
        return TestResults.LastOrDefault(t => t.Status == TestStatus.Running);
    }

    private static string GetRequestBody(RestRequest request)
    {
        var bodyParam = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
        if (bodyParam?.Value != null)
        {
            return JsonConvert.SerializeObject(bodyParam.Value, Formatting.Indented);
        }
        return "";
    }

    private static Dictionary<string, string> GetResponseHeaders(RestResponse response)
    {
        var headerDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                if (header.Name != null)
                {
                    // Handle duplicate headers by combining values with comma
                    if (headerDict.ContainsKey(header.Name))
                    {
                        headerDict[header.Name] += ", " + (header.Value?.ToString() ?? "");
                    }
                    else
                    {
                        headerDict[header.Name] = header.Value?.ToString() ?? "";
                    }
                }
            }
        }
        return headerDict;
    }

    private static string GenerateHtmlReport(object reportData)
    {
        var json = JsonConvert.SerializeObject(reportData, Formatting.Indented);
        
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ZenQA API Test Report</title>
    <style>
        {GetCssStyles()}
    </style>
</head>
<body>
    <div class=""container"">
        <header class=""header"">
            <h1>🧪 ZenQA API Test Report</h1>
            <div class=""generated-info"">Generated on: <span id=""generatedAt""></span></div>
        </header>
        
        <div class=""summary-section"">
            <div class=""summary-card"">
                <h2>📊 Test Summary</h2>
                <div class=""summary-grid"" id=""summaryGrid""></div>
            </div>
        </div>
        
        <div class=""tests-section"">
            <h2>🔍 Test Details</h2>
            <div id=""testDetails""></div>
        </div>
    </div>
    
    <script>
        const reportData = {json};
        {GetJavaScript()}
    </script>
</body>
</html>";
    }

    private static string GetCssStyles()
    {
        return @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f5f7fa; color: #333; line-height: 1.6; }
        .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
        .header { text-align: center; margin-bottom: 30px; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 10px; }
        .header h1 { font-size: 2.5em; margin-bottom: 10px; }
        .generated-info { font-size: 1.1em; opacity: 0.9; }
        .summary-section { margin-bottom: 40px; }
        .summary-card { background: white; padding: 30px; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
        .summary-card h2 { margin-bottom: 20px; color: #4a5568; font-size: 1.8em; }
        .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; }
        .summary-item { text-align: center; padding: 20px; border-radius: 10px; }
        .summary-item.total { background: linear-gradient(135deg, #3182ce, #2c5aa0); color: white; }
        .summary-item.passed { background: linear-gradient(135deg, #38a169, #2f855a); color: white; }
        .summary-item.failed { background: linear-gradient(135deg, #e53e3e, #c53030); color: white; }
        .summary-item.skipped { background: linear-gradient(135deg, #ed8936, #dd6b20); color: white; }
        .summary-item.duration { background: linear-gradient(135deg, #805ad5, #6b46c1); color: white; }
        .summary-item.passrate { background: linear-gradient(135deg, #38b2ac, #319795); color: white; }
        .summary-value { font-size: 2.5em; font-weight: bold; margin-bottom: 5px; }
        .summary-label { font-size: 1.1em; opacity: 0.9; }
        .tests-section h2 { margin-bottom: 25px; color: #4a5568; font-size: 1.8em; }
        .test-card { background: white; margin-bottom: 20px; border-radius: 15px; overflow: hidden; box-shadow: 0 5px 15px rgba(0,0,0,0.08); }
        .test-header { padding: 20px; cursor: pointer; display: flex; justify-content: between; align-items: center; }
        .test-header.passed { border-left: 5px solid #38a169; background: linear-gradient(90deg, #f0fff4, #ffffff); }
        .test-header.failed { border-left: 5px solid #e53e3e; background: linear-gradient(90deg, #fed7d7, #ffffff); }
        .test-info { flex: 1; }
        .test-name { font-size: 1.3em; font-weight: 600; margin-bottom: 5px; }
        .test-meta { color: #666; font-size: 0.95em; }
        .test-status { font-size: 1.5em; margin-left: 20px; }
        .test-details { padding: 0 20px 20px 20px; display: none; }
        .test-details.expanded { display: block; }
        .request-response { margin-top: 15px; }
        .http-section { margin-bottom: 20px; }
        .http-section h4 { margin-bottom: 10px; color: #4a5568; }
        .code-block { background: #f7fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 15px; font-family: 'Courier New', monospace; font-size: 0.9em; overflow-x: auto; }
        .error-section { background: #fed7d7; border: 1px solid #feb2b2; border-radius: 8px; padding: 15px; margin-top: 15px; }
        .error-message { color: #c53030; font-weight: 600; margin-bottom: 10px; }
        .stack-trace { color: #744210; font-family: 'Courier New', monospace; font-size: 0.85em; white-space: pre-wrap; }
        .method-badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 0.8em; font-weight: bold; margin-right: 10px; color: white; }
        .method-GET { background: #38a169; }
        .method-POST { background: #3182ce; }
        .method-PUT { background: #ed8936; }
        .method-PATCH { background: #805ad5; }
        .method-DELETE { background: #e53e3e; }
        .status-badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 0.8em; font-weight: bold; color: white; }
        .status-2xx { background: #38a169; }
        .status-4xx { background: #ed8936; }
        .status-5xx { background: #e53e3e; }
        ";
    }

    private static string GetJavaScript()
    {
        return @"
        function initializeReport() {
            // Display generated time
            document.getElementById('generatedAt').textContent = new Date(reportData.GeneratedAt).toLocaleString();
            
            // Display summary
            const summaryGrid = document.getElementById('summaryGrid');
            summaryGrid.innerHTML = `
                <div class=""summary-item total"">
                    <div class=""summary-value"">${reportData.Summary.TotalTests}</div>
                    <div class=""summary-label"">Total Tests</div>
                </div>
                <div class=""summary-item passed"">
                    <div class=""summary-value"">${reportData.Summary.Passed}</div>
                    <div class=""summary-label"">Passed</div>
                </div>
                <div class=""summary-item failed"">
                    <div class=""summary-value"">${reportData.Summary.Failed}</div>
                    <div class=""summary-label"">Failed</div>
                </div>
                <div class=""summary-item skipped"">
                    <div class=""summary-value"">${reportData.Summary.Skipped}</div>
                    <div class=""summary-label"">Skipped</div>
                </div>
                <div class=""summary-item duration"">
                    <div class=""summary-value"">${Math.round(reportData.Summary.TotalDuration)}ms</div>
                    <div class=""summary-label"">Total Duration</div>
                </div>
                <div class=""summary-item passrate"">
                    <div class=""summary-value"">${reportData.Summary.PassRate.toFixed(1)}%</div>
                    <div class=""summary-label"">Pass Rate</div>
                </div>
            `;
            
            // Display test details
            const testDetails = document.getElementById('testDetails');
            testDetails.innerHTML = reportData.Tests.map(test => generateTestHtml(test)).join('');
        }
        
        function generateTestHtml(test) {
            const statusClass = test.Status.toLowerCase();
            const statusIcon = test.Status === 'Passed' ? '✅' : test.Status === 'Failed' ? '❌' : '⏭️';
            const duration = test.Duration ? Math.round(test.Duration.TotalMilliseconds) : 0;
            
            return `
                <div class=""test-card"">
                    <div class=""test-header ${statusClass}"" onclick=""toggleTestDetails('${test.TestName}')"">
                        <div class=""test-info"">
                            <div class=""test-name"">${test.TestName}</div>
                            <div class=""test-meta"">
                                Duration: ${duration}ms | 
                                Requests: ${test.HttpRequests?.length || 0} | 
                                Started: ${new Date(test.StartTime).toLocaleTimeString()}
                            </div>
                        </div>
                        <div class=""test-status"">${statusIcon}</div>
                    </div>
                    <div class=""test-details"" id=""details-${test.TestName}"">
                        ${test.Description ? `<p><strong>Description:</strong> ${test.Description}</p>` : ''}
                        ${test.HttpRequests?.map(req => generateRequestHtml(req)).join('') || ''}
                        ${test.Status === 'Failed' && test.ErrorMessage ? `
                            <div class=""error-section"">
                                <div class=""error-message"">${test.ErrorMessage}</div>
                                ${test.StackTrace ? `<div class=""stack-trace"">${test.StackTrace}</div>` : ''}
                            </div>
                        ` : ''}
                    </div>
                </div>
            `;
        }
        
        function generateRequestHtml(request) {
            if (!request.Response) return '';
            
            const statusClass = request.Response.StatusCode >= 200 && request.Response.StatusCode < 300 ? 'status-2xx' :
                              request.Response.StatusCode >= 400 && request.Response.StatusCode < 500 ? 'status-4xx' : 'status-5xx';
            
            return `
                <div class=""request-response"">
                    <div class=""http-section"">
                        <h4>
                            <span class=""method-badge method-${request.Method}"">${request.Method}</span>
                            ${request.Endpoint}
                            <span class=""status-badge ${statusClass}"">${request.Response.StatusCode} ${request.Response.StatusDescription}</span>
                            <span style=""float: right; color: #666;"">${Math.round(request.Response.Duration.TotalMilliseconds)}ms</span>
                        </h4>
                        ${request.Body ? `
                            <div>
                                <strong>Request Body:</strong>
                                <div class=""code-block"">${formatJson(request.Body)}</div>
                            </div>
                        ` : ''}
                        <div>
                            <strong>Response Body:</strong>
                            <div class=""code-block"">${formatJson(request.Response.Body)}</div>
                        </div>
                    </div>
                </div>
            `;
        }
        
        function toggleTestDetails(testName) {
            const details = document.getElementById(`details-${testName}`);
            details.classList.toggle('expanded');
        }
        
        function formatJson(jsonString) {
            try {
                const parsed = JSON.parse(jsonString);
                return JSON.stringify(parsed, null, 2);
            } catch {
                return jsonString;
            }
        }
        
        // Initialize the report when the page loads
        document.addEventListener('DOMContentLoaded', initializeReport);
        ";
    }

    public static void ClearResults()
    {
        TestResults.Clear();
    }
}

public class TestExecutionDetails
{
    public string TestName { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public TestStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public List<RequestLogDetails> HttpRequests { get; set; } = new();
}

public class RequestLogDetails
{
    public string Method { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public ResponseLogDetails? Response { get; set; }
}

public class ResponseLogDetails
{
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = "";
    public string ContentType { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum TestStatus
{
    Running,
    Passed,
    Failed,
    Skipped
}