# Mortgage Application API

A small ASP.NET Core 10 JSON API for mortgage applications and affordability estimates.

## Setup and run

Install the .NET 10 SDK. From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project .\MortgageApplication.Api\MortgageApplication.Api.csproj --launch-profile https
```

The API listens at `https://localhost:7133` and `http://localhost:5251`. Submitted applications are saved to `MortgageApplication.Api/App_Data/mortgage-applications.json`; the file is created as needed and is not committed.

The `MortgageApplication.Api/MortgageApplication.Api.http` file has sample requests for Visual Studio:

- `POST /api/mortgage-applications` — create an application
- `GET /api/mortgage-applications/{id}` — retrieve it
- `GET /api/mortgage-applications/{id}/affordability` — assess it

Create returns `201`; invalid input returns `400`; an unknown ID returns `404`; an uncalculable assessment returns `422`.

## Console client

With the API running, use a second terminal at the repository root:

```powershell
dotnet run --project .\console\MortgageApplication.Client\MortgageApplication.Client.csproj
```

The client defaults to `http://localhost:5251` and lets you create, retrieve, and assess an application.

## Tests

```powershell
dotnet test .\tests\MortgageApplication.Tests\MortgageApplication.Tests.csproj
```

Unit tests cover affordability calculations, request validation, repository behaviour, and controller outcomes.

## Affordability rules

The estimate uses a fixed 5% annual rate with monthly repayments. Monthly expenses are existing costs and exclude the new mortgage payment. DTI is `(monthly expenses + mortgage payment) / gross monthly income`; LTV is `requested loan / property value`.

Approval requires DTI at or below 40% and LTV at or below 90%. Maximum borrowing is the lower of the amount supported by the remaining DTI payment budget and 90% of the property value. These are simplified assumptions for the exercise.

## Design notes

- Validate requests at the API boundary, including that the deposit does not exceed the property value, so invalid applications are rejected before assessment.
- Keep HTTP handling, business logic, and persistence in separate classes so calculations can be tested without the API or file store.
- Use a JSON file to keep this exercise simple. The write lock protects only one API process; for concurrent access across multiple instances, I would replace it with a database.
- Authentication and lender-specific underwriting are outside this exercise. With more time, I would add API integration tests and make the rate and limits configurable.
