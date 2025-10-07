#  ZenQA REST API Automation Framework

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![NUnit](https://img.shields.io/badge/NUnit-3.14.0-green.svg)](https://nunit.org/)
[![RestSharp](https://img.shields.io/badge/RestSharp-112.1.0-orange.svg)](https://restsharp.dev/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A comprehensive, enterprise-grade REST API test automation framework built with C#, NUnit, and RestSharp. Features advanced reporting, detailed logging, and comprehensive test coverage for the `restful-api.dev` sample API.

##  Quick Start

### Prerequisites
- **.NET 9.0 SDK** or later ([Download here](https://dotnet.microsoft.com/download))
- **Git** for version control
- **Visual Studio Code** or **Visual Studio** (recommended)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ZenQA.RestApiAutomation-fixed
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run all tests**
   ```bash
   dotnet test
   ```

4. **Generate detailed reports**
   ```bash
   dotnet test --logger "html;LogFileName=test-report.html" --results-directory tests/ZenQA.ApiTests/reports
   ```

## Project Structure

```
ZenQA.RestApiAutomation-fixed/
├── ZenQA.RestApiAutomation.sln              # Solution file
├── README.md                                 # This file
├── tests/
│   └── ZenQA.ApiTests/                      # Main test project
│       ├── Common/                          # Shared utilities and base classes
│       │   ├── ApiClient.cs                 # HTTP client configuration
│       │   ├── RequestBuilder.cs            # Fluent API request builder
│       │   ├── TestBase.cs                  # Base test class with setup/teardown
│       │   ├── TestConfig.cs                # Configuration management
│       │   └── TestReporter.cs              # Advanced test reporting
│       ├── TestSpecs/                       # Test specifications organized by functionality
│       │   ├── Objects_Create_Specs.cs      # POST /objects tests
│       │   ├── Objects_Get_Specs.cs         # GET /objects tests
│       │   ├── Objects_Update_Specs.cs      # PATCH /objects tests
│       │   └── Objects_Delete_Specs.cs      # DELETE /objects tests
│       ├── HealthChecks/                    # Smoke and health check tests
│       │   └── Health_Smoke_Specs.cs        # API availability tests
│       ├── reports/                         # Generated test reports
│       │   ├── comprehensive-test-report.html  # Detailed interactive report
│       │   └── test-execution.log           # Detailed execution logs
│       ├── appsettings.json                 # Environment configuration
│       └── ZenQA.ApiTests.csproj           # Project file with dependencies
└── .github/
    └── workflows/
        └── ci-cd-pipeline.yml               # GitHub Actions CI/CD workflow
```

## Tools & Technologies

### Core Framework
- **.NET 9.0** - Latest .NET runtime for optimal performance
- **NUnit 3.14.0** - Modern unit testing framework with parallel execution
- **RestSharp 112.1.0** - Powerful HTTP client library (latest secure version)
- **FluentAssertions 6.12.0** - Expressive assertion library

### Reporting & Logging
- **Serilog 4.3.0** - Structured logging with file output
- **ReportGenerator 5.4.17** - Advanced test coverage reporting
- **JunitXml.TestLogger 3.0.134** - CI/CD compatible XML reports
- **Custom HTML Reporter** - Interactive test reports with HTTP request/response details

### Development & CI/CD
- **GitHub Actions** - Automated testing and deployment pipeline
- **coverlet.collector** - Code coverage analysis
- **Newtonsoft.Json 13.0.3** - JSON serialization and manipulation

## Test Strategy & Coverage

### API Endpoints Tested
Our test suite provides comprehensive coverage of the RESTful API lifecycle:

| HTTP Method | Endpoint | Test Scenarios | Status Codes Tested |
|-------------|----------|---------------|-------------------|
| **GET** | `/objects` | List all objects, pagination | 200 OK |
| **GET** | `/objects/{id}` | Get single object, non-existent ID | 200 OK, 404 Not Found |
| **POST** | `/objects` | Create new object, validate response | 200 OK |
| **PATCH** | `/objects/{id}` | Partial update, field preservation, invalid payload | 200 OK, 400 Bad Request, 404 Not Found |
| **DELETE** | `/objects/{id}` | Delete object, cleanup operations | 200 OK, 204 No Content |

### Test Categories

#### **Smoke Tests** (`HealthChecks/`)
- **API Availability**: Verify base URL is reachable
- **Service Health**: Ensure API responds within acceptable timeframes
- **Environment Validation**: Confirm test environment is properly configured

#### **Create Operations** (`TestSpecs/Objects_Create_Specs.cs`)
- **Positive Cases**: Valid object creation with proper response validation
- **Data Integrity**: Verify created objects contain all expected fields
- **Response Structure**: Validate JSON schema and data types

#### **Read Operations** (`TestSpecs/Objects_Get_Specs.cs`)
- **List Retrieval**: Get all objects and validate array response
- **Single Object**: Retrieve specific objects by ID
- **Error Handling**: Test with non-existent IDs (404 scenarios)

#### **Update Operations** (`TestSpecs/Objects_Update_Specs.cs`)
- **Partial Updates**: PATCH with selective field updates
- **Field Preservation**: Ensure unmodified fields remain intact
- **Error Scenarios**: Invalid payloads, non-existent resources
- **Data Validation**: Verify update operations persist correctly

#### **Delete Operations** (`TestSpecs/Objects_Delete_Specs.cs`)
- **Successful Deletion**: Remove objects and verify response
- **Cleanup Validation**: Ensure deleted objects are no longer accessible
- **Idempotency**: Test delete operations on already deleted resources

### Assertion Strategy
- **HTTP Status Code Validation**: Comprehensive status code testing (200, 400, 404, etc.)
- **Response Schema Validation**: JSON structure and data type verification
- **Data Integrity Checks**: Ensure CRUD operations maintain data consistency
- **Performance Assertions**: Response time monitoring and validation
- **Semantic Validation**: Business logic verification (e.g., PATCH field preservation)

## Configuration Management

### Environment Configuration
The framework supports multiple environments through `appsettings.json`:

```json
{
  "environment": "DEV",
  "DEV": {
    "baseUrl": "https://api.restful-api.dev",
    "defaultHeaders": {}
  },
  "STAGE": {
    "baseUrl": "https://api.staging-restful-api.dev",
    "defaultHeaders": {
      "Authorization": "Bearer staging-token"
    }
  }
}
```

### Runtime Configuration Override
```bash
# Override environment
set TEST_ENV=STAGE

# Override base URL
set API_BASE_URL=https://custom-api-endpoint.com

# Run tests with custom configuration
dotnet test
```

##  Advanced Reporting

### Generated Reports

1. **Comprehensive HTML Report** (`reports/comprehensive-test-report.html`)
   - Interactive test details with expand/collapse functionality
   - HTTP request/response data for each test
   - Performance metrics and response times
   - Visual status indicators and method badges
   - API coverage summary

2. **Standard HTML Report** (`reports/test-report.html`)
   - Standard test execution results
   - Pass/fail statistics
   - Duration and performance data

3. **XML Reports** (JUnit format)
   - CI/CD compatible format
   - Integration with build pipelines
   - Test result artifacts

4. **Execution Logs** (`reports/test-execution.log`)
   - Detailed structured logging
   - HTTP request/response logging
   - Performance tracking
   - Error diagnostics

### Report Features
-  **100% Pass Rate Tracking**
-  **Response Time Analysis**
-  **API Coverage Metrics**
-  **Request/Response Inspection**
-  **Performance Trends**

##  CI/CD Pipeline (GitHub Actions)

### Automated Workflow Features
- **Multi-OS Testing**: Windows, macOS, Linux compatibility
- **Parallel Test Execution**: Optimized for speed
- **Automatic Report Generation**: HTML and XML reports
- **Artifact Publishing**: Test results and logs
- **Slack/Email Notifications**: Test result notifications
- **Performance Monitoring**: Response time tracking

### Workflow Triggers
- **Push to main/develop branches**
- **Pull request validation**
- **Scheduled runs** (daily/weekly)
- **Manual execution**

##  Running Tests

### Basic Execution
```bash
# Run all tests
dotnet test

# Run with detailed console output
dotnet test --logger "console;verbosity=detailed"

# Run specific test category
dotnet test --filter "Category=Smoke"
```

### Advanced Execution Options
```bash
# Generate comprehensive reports
dotnet test --logger "html;LogFileName=test-report.html" --results-directory tests/ZenQA.ApiTests/reports

# Run with coverage analysis
dotnet test --collect:"XPlat Code Coverage"

# Parallel execution with custom settings
dotnet test --parallel --logger "junit;LogFilePath=results.xml"
```

### Environment-Specific Testing
```bash
# Test against staging environment
TEST_ENV=STAGE dotnet test

# Test with custom API endpoint
API_BASE_URL=https://custom-api.com dotnet test
```

##  Development Guidelines

### Adding New Tests
1. **Create test class** in appropriate `TestSpecs/` folder
2. **Inherit from TestBase** for automatic setup/teardown
3. **Use descriptive test names** following convention: `{operation}_{should}_{expected_result}`
4. **Add comprehensive assertions** using FluentAssertions
5. **Include both positive and negative test cases**

### Code Standards
- **Async/await patterns** for all HTTP operations
- **Fluent assertion style** for readable test code
- **Comprehensive error handling** and logging
- **DRY principle** - reuse common operations
- **Clear separation** between test setup, execution, and assertion

##  Key Features

-  **Comprehensive API Testing**: Full CRUD operation coverage
-  **Modular Architecture**: Easy to extend and maintain
-  **Advanced Reporting**: Interactive HTML reports with request/response details
-  **CI/CD Ready**: GitHub Actions integration
-  **Security Focused**: Latest package versions with vulnerability scanning
-  **Performance Optimized**: Parallel test execution
-  **Multi-Environment**: Configurable for different environments
-  **Detailed Logging**: Structured logging with Serilog
-  **BDD Style**: Readable test specifications

##  Contributing

1. **Fork the repository**
2. **Create feature branch**: `git checkout -b feature/amazing-feature`
3. **Add comprehensive tests** for new functionality
4. **Ensure all tests pass**: `dotnet test`
5. **Update documentation** as needed
6. **Submit pull request** with detailed description

##  License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

##  Support & Troubleshooting

### Common Issues
- **SSL Certificate Errors**: Update certificates or configure RestSharp to ignore SSL validation in test environments
- **Network Timeouts**: Adjust timeout settings in `ApiClient.cs`
- **Test Flakiness**: Review test data dependencies and add proper cleanup

### Getting Help
- **Create GitHub Issue** for bugs or feature requests
- **Check Logs**: Review `test-execution.log` for detailed error information
- **Run with Verbose Logging**: Use `--logger "console;verbosity=detailed"`

---

**Built with the ZenQA Team**