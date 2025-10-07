# ZenQA REST API Automation (C# · NUnit · RestSharp)

A clean, modular API test framework targeting the `restful-api.dev` sample API.

## Structure

```
ZenQA.RestApiAutomation.sln
tests/
  ZenQA.ApiTests/
    Common/
      ApiClient.cs
      RequestBuilder.cs
      TestBase.cs
      TestConfig.cs
    Specs/
      Objects_Create_Specs.cs
      Objects_Get_Specs.cs
      Objects_Update_Specs.cs
      Objects_Delete_Specs.cs
    Smoke/
      Health_Smoke_Specs.cs
    appsettings.json
    reports/          # junit.xml uploaded by CI
.github/workflows/
  dotnet-tests.yml
```

## Run locally

```bash
# .NET 8 SDK is required
dotnet restore ZenQA.RestApiAutomation.sln

# (optional) override environment
# set TEST_ENV=DEV
# set API_BASE_URL=https://api.restful-api.dev

dotnet test ZenQA.RestApiAutomation.sln --logger "console;verbosity=detailed"
```

## Config / Env

- `tests/ZenQA.ApiTests/appsettings.json` contains base URLs per environment.
- Override at runtime:
  - `TEST_ENV` (e.g., `DEV`, `STAGE`)
  - `API_BASE_URL` (absolute URL takes precedence over env map)

## Tools & Packages

- .NET 8, NUnit, RestSharp, FluentAssertions, Newtonsoft.Json
- JUnitXml.TestLogger (for CI artifact)

## CI/CD (GitHub Actions)

- Workflow: `.github/workflows/dotnet-tests.yml`
- Restores, builds, runs tests; publishes `reports/junit.xml` as an artifact

## Test Strategy & Coverage

Endpoints covered against `https://api.restful-api.dev`:
- **POST** `/objects` (create)
- **GET** `/objects/{id}` & `/objects` (read)
- **PATCH** `/objects/{id}` (partial update) — **positive + negative**
- **DELETE** `/objects/{id}` (cleanup)

### Assertions

- HTTP status codes, response schema fragments, semantic checks (PATCH preserves untouched fields).

### Observations

- `PATCH` keeps unspecified fields intact (partial update semantics).
- Non-existent IDs return 404.
- Invalid payloads return a 4xx (implementation-dependent).

## Notes

This repository is intentionally minimal, focusing on clean test code, configuration hygiene, and CI.