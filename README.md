# Mortgage Application API

A small ASP.NET Core 10 mortgage application system with a JSON API and Blazor web front end.

## Setup and run

Install the .NET 10 SDK, then restore and build from the repository root:

```powershell
dotnet restore
dotnet build
```

Start the API in one terminal:

```powershell
$env:Authentication__ApiKey = "replace-with-a-long-random-key"
dotnet run --project .\MortgageApplication.Api\MortgageApplication.Api.csproj --launch-profile http
```

It listens at `http://localhost:5251`. Submitted applications are saved to `MortgageApplication.Api/App_Data/mortgage-applications.json`; the file is created as needed and is not committed.

In a second terminal, start the Blazor front end:

```powershell
$env:Api__ApiKey = "replace-with-a-long-random-key"
dotnet run --project .\MortgageApplication.Web\MortgageApplication.Web.csproj --launch-profile http
```

Open `http://localhost:5260`. The page submits applications, looks up an application ID, and requests its affordability estimate through the API. Set the same key in both terminals. The Blazor server sends it to the API; it is not included in browser requests. The API address can be changed in `MortgageApplication.Web/appsettings.json`.

The `MortgageApplication.Api/MortgageApplication.Api.http` file has sample requests for Visual Studio. Replace `REPLACE_WITH_API_KEY` with the configured key:

- `POST /api/mortgage-applications` — create an application
- `GET /api/mortgage-applications/{id}` — retrieve it
- `GET /api/mortgage-applications/{id}/affordability` — assess it

Create returns `201`; invalid input returns `400`; a request without a valid API key returns `401`; an unknown ID returns `404`; an uncalculable assessment returns `422`.

## Tests

```powershell
dotnet test .\tests\MortgageApplication.Tests\MortgageApplication.Tests.csproj
```

Unit tests cover calculations, validation, repository behaviour, controller outcomes, and generic server error responses.

## Affordability rules

The estimate uses a fixed 5% annual rate with monthly repayments. Monthly expenses are existing costs and exclude the new mortgage payment. DTI is `(monthly expenses + mortgage payment) / gross monthly income`; LTV is `requested loan / property value`.

Approval requires DTI at or below 40% and LTV at or below 90%. Maximum borrowing is the lower of the amount supported by the remaining DTI payment budget and 90% of the property value. These are simplified assumptions for the exercise.

## Design notes

- Validate requests at the API boundary, including that the deposit does not exceed the property value, so invalid applications are rejected before assessment.
- Keep HTTP handling, business logic, and persistence in separate classes so calculations can be tested without the API or file store.
- Use a JSON file to keep this exercise simple. The write lock protects only one API process; for concurrent access across multiple instances, I would replace it with a database.
- Unexpected API exceptions are logged server-side and returned with a generic message. Expected validation, missing-record, and calculation-limit responses keep their HTTP status without exposing exception details.
- The API key protects service-to-service requests; customer accounts, roles, and key rotation are beyond this exercise. With more time, I would add API integration tests and make the rate and limits configurable.
