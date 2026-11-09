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

## Design and reflection

### Key decisions

- Validate requests at the API boundary and keep HTTP handling, calculations, and storage in separate classes. This keeps invalid applications out of the assessment logic and makes the calculations testable on their own.
- Use a JSON file for simple, inspectable local persistence. Its write lock only coordinates one API process.
- Protect API requests with a configured API key. Unexpected errors are logged on the server and returned to clients with a generic message.

### Scaling

The first change would be replacing the JSON file with a database. The current repository reads and rewrites the full file and only coordinates one process, which limits throughput and prevents safe multi-instance use.

### Trade-offs and next steps

The rate, DTI and LTV limits are fixed examples; this does not implement lender-specific underwriting or customer accounts. With more time, I would add API integration tests, make the assumptions configurable, and add customer identity and key rotation.
