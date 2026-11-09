# Mortgage Application API

A small ASP.NET Core 10 JSON API for submitting mortgage applications, retrieving them, and generating a basic affordability estimate.

## Requirements

- .NET 10 SDK

## Run the application

From the repository root:

```powershell
dotnet run --project .\MortgageApplication.Api\MortgageApplication.Api.csproj
```

The HTTPS launch profile listens at `https://localhost:7133` and `http://localhost:5251`.

The application stores submitted applications in `MortgageApplication.Api/App_Data/mortgage-applications.json`. This file is created on first submission and is excluded from Git.

## API

### Create an application

`POST /api/mortgage-applications`

```json
{
  "annualIncome": 100000,
  "monthlyExpenses": 1200,
  "depositAmount": 100000,
  "propertyValue": 400000,
  "termYears": 25
}
```

Returns `201 Created`, the new application, and a `Location` header for retrieving it. Invalid values return `400 Bad Request` with validation details.

### Retrieve an application

`GET /api/mortgage-applications/{id}`

Returns `200 OK` or `404 Not Found`.

### Assess affordability

`GET /api/mortgage-applications/{id}/affordability`

Returns `200 OK` with LTV, DTI, approval decision, maximum borrowing estimate, and an explanation. Returns `404 Not Found` when the application does not exist.
If the financial values exceed the range supported by decimal calculations, the API returns `422 Unprocessable Entity`.

The `MortgageApplication.Api.http` file contains example requests that can be run from Visual Studio.

## Customer console client

The small interactive console client exercises the same create, retrieve, and affordability endpoints as an API customer. Start the API in one terminal:

```powershell
dotnet run --project .\MortgageApplication.Api\MortgageApplication.Api.csproj --launch-profile http
```

Then start the client in another terminal:

```powershell
dotnet run --project .\console\MortgageApplication.Client\MortgageApplication.Client.csproj
```

The client defaults to `http://localhost:5251`; enter a different base URL when prompted if the API is hosted elsewhere.

## Affordability assumptions

The assessment is illustrative and is not a lending decision. It assumes:

- A fixed annual interest rate of 5% and equal monthly repayments over the requested term.
- Monthly expenses are existing non-mortgage obligations and do not include the new mortgage payment.
- DTI is `(monthly expenses + estimated mortgage payment) / gross monthly income`. Ratios are returned as fractions, so `0.35` means 35%.
- An application is approved when DTI is at most 40% and LTV is at most 90%.
- Maximum borrowing is the lower of the principal supported by the remaining 40% DTI payment budget and 90% of the supplied property value.
- Taxes, insurance, property costs, credit history, and other underwriting factors are outside scope.

## Tests

Run the automated unit tests from the repository root:

```powershell
dotnet test .\tests\MortgageApplication.Tests\MortgageApplication.Tests.csproj
```

The tests cover affordability outcomes and core request validation rules.

## Design decisions

1. **Keep the assessment as a separate service.** `AffordabilityAssessor` contains the calculation rules without depending on HTTP or persistence, so the core logic is easy to exercise with unit tests.
2. **Put storage behind a repository interface.** The API and application service do not depend on the JSON file implementation directly. The file store preserves applications between restarts and uses a lock plus replace-on-write for safe single-process access.
3. **Validate at the request boundary.** Data annotations cover field ranges and `IValidatableObject` checks the relationship between deposit and property value. ASP.NET's `[ApiController]` returns structured validation errors automatically.

## Scaling consideration

The first change for higher load would be replacing the JSON file repository with a database-backed implementation, such as PostgreSQL or SQL Server, while keeping the repository boundary. A database would support multiple API instances, concurrent writes, indexed lookups, and operational backup and recovery. The current file store is intentionally for a single-instance exercise and is not suitable for multi-instance deployment.

## Trade-offs and next steps

This solution keeps persistence and underwriting deliberately small. It does not include authentication, migrations, lender-specific rules, rate configuration, or production observability. With more time, I would add API integration tests, structured request logging and metrics, configurable assumptions, and a database repository with migration and backup guidance.
