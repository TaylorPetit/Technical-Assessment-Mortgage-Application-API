# Mortgage Application API

A small ASP.NET Core 10 JSON API for creating mortgage applications and estimating affordability.

## Run it

Install the .NET 10 SDK. From the repository root, start the API:

```powershell
dotnet run --project .\MortgageApplication.Api\MortgageApplication.Api.csproj --launch-profile https
```

It listens at `https://localhost:7133` and `http://localhost:5251`. Applications are stored in `MortgageApplication.Api/App_Data/mortgage-applications.json`; the file is created as needed and is not committed.

To try the API directly, open `MortgageApplication.Api/MortgageApplication.Api.http` in Visual Studio. The endpoints are:

- `POST /api/mortgage-applications` — create an application
- `GET /api/mortgage-applications/{id}` — retrieve it
- `GET /api/mortgage-applications/{id}/affordability` — assess it

## Console client

With the API running, open a second terminal at the repository root:

```powershell
dotnet run --project .\console\MortgageApplication.Client\MortgageApplication.Client.csproj
```

The client calls the same three endpoints and defaults to `http://localhost:5251`.

## Tests

```powershell
dotnet test .\tests\MortgageApplication.Tests\MortgageApplication.Tests.csproj
```

Tests cover the affordability calculations, request validation, repository behaviour, and controller responses.

## Calculation

The estimate uses a fixed 5% annual rate with monthly repayments. Monthly expenses are existing costs and do not include the new mortgage payment. DTI is `(monthly expenses + mortgage payment) / gross monthly income`.

Approval requires DTI at or below 40% and LTV at or below 90%. Maximum borrowing is the lower of the income-based amount and 90% of the property value. These are simple rules for this exercise.

## Design notes

- Request validation happens at the API boundary, including a check that the deposit is below the property value.
- The controller, service, assessor, and repository have separate jobs, which keeps calculations and storage easier to test or change.
- JSON storage avoids database setup for this exercise, but its lock only protects one API process. For multiple instances, the first change would be a database.
- With more time, I would add API integration tests and make the rate and limits configurable.
