using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

// From the repository root, start the API and client in separate terminals:
// dotnet run --project MortgageApplication.Api/MortgageApplication.Api.csproj --launch-profile http
// dotnet run --project console/MortgageApplication.Client/MortgageApplication.Client.csproj
// The client defaults to http://localhost:5251.

var baseAddress = ReadBaseAddress();
using var http = new HttpClient { BaseAddress = baseAddress };

Console.WriteLine("Mortgage Application customer test client");
Console.WriteLine($"API: {http.BaseAddress}");

while (true)
{
    Console.WriteLine("\n1. Submit a mortgage application");
    Console.WriteLine("2. Retrieve an application");
    Console.WriteLine("3. Check affordability");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            await CreateApplicationAsync(http);
            break;
        case "2":
            await RetrieveApplicationAsync(http);
            break;
        case "3":
            await AssessAffordabilityAsync(http);
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Please choose 1, 2, 3, or 0.");
            break;
    }
}

static Uri ReadBaseAddress()
{
    Console.Write("API base URL [http://localhost:5251]: ");
    var input = Console.ReadLine()?.Trim();
    var value = string.IsNullOrWhiteSpace(input) ? "http://localhost:5251" : input;
    if (!Uri.TryCreate(value, UriKind.Absolute, out var address) ||
        (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
    {
        Console.WriteLine("That URL is invalid. Using http://localhost:5251.");
        return new Uri("http://localhost:5251/");
    }

    return new Uri(value.EndsWith('/') ? value : value + "/");
}

static async Task CreateApplicationAsync(HttpClient http)
{
    var request = new CreateApplicationRequest(
        ReadDecimal("Gross annual income"),
        ReadDecimal("Monthly expenses"),
        ReadDecimal("Deposit amount"),
        ReadDecimal("Property value"),
        ReadInt("Mortgage term in years"));

    try
    {
        using var response = await http.PostAsJsonAsync("api/mortgage-applications", request);
        if (!response.IsSuccessStatusCode)
        {
            await ShowErrorAsync(response);
            return;
        }

        var application = await response.Content.ReadFromJsonAsync<MortgageApplicationResponse>();
        if (application is null)
        {
            Console.WriteLine("The API returned an empty response.");
            return;
        }

        Console.WriteLine("Application submitted successfully.");
        ShowApplication(application);
        Console.WriteLine("Save this application ID to retrieve it or check affordability later.");
    }
    catch (HttpRequestException ex)
    {
        ShowConnectionError(ex);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Could not read the API response: {ex.Message}");
    }
}

static async Task RetrieveApplicationAsync(HttpClient http)
{
    var id = ReadApplicationId();
    if (id is null) return;

    try
    {
        var application = await http.GetFromJsonAsync<MortgageApplicationResponse>($"api/mortgage-applications/{id}");
        if (application is null)
        {
            Console.WriteLine("The API returned an empty response.");
            return;
        }

        ShowApplication(application);
    }
    catch (HttpRequestException ex)
    {
        if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            Console.WriteLine("No application was found with that ID.");
        else
            ShowConnectionError(ex);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Could not read the API response: {ex.Message}");
    }
}

static async Task AssessAffordabilityAsync(HttpClient http)
{
    var id = ReadApplicationId();
    if (id is null) return;

    try
    {
        var assessment = await http.GetFromJsonAsync<AffordabilityResponse>(
            $"api/mortgage-applications/{id}/affordability");
        if (assessment is null)
        {
            Console.WriteLine("The API returned an empty response.");
            return;
        }

        Console.WriteLine($"Application: {assessment.ApplicationId}");
        Console.WriteLine($"Affordability result: {(assessment.Approved ? "Within the illustrative limits" : "Outside the illustrative limits")}");
        Console.WriteLine($"Loan-to-value: {assessment.LoanToValueRatio:P1}");
        Console.WriteLine($"Debt-to-income: {assessment.DebtToIncomeRatio:P1}");
        Console.WriteLine($"Estimated maximum borrowing: {assessment.MaximumBorrowingEstimate.ToString("C0", CultureInfo.GetCultureInfo("en-CA"))}");
        Console.WriteLine(assessment.Explanation);
    }
    catch (HttpRequestException ex)
    {
        if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            Console.WriteLine("No application was found with that ID.");
        else if (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            Console.WriteLine("The API could not calculate an assessment for these financial values.");
        else
            ShowConnectionError(ex);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Could not read the API response: {ex.Message}");
    }
}

static Guid? ReadApplicationId()
{
    Console.Write("Application ID: ");
    if (Guid.TryParse(Console.ReadLine()?.Trim(), out var id)) return id;
    Console.WriteLine("Enter a valid application ID (GUID).");
    return null;
}

static decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine();
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ||
            decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return value;
        Console.WriteLine("Enter a valid number.");
    }
}

static int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        if (int.TryParse(Console.ReadLine(), out var value)) return value;
        Console.WriteLine("Enter a whole number.");
    }
}

static void ShowApplication(MortgageApplicationResponse application)
{
    var cad = CultureInfo.GetCultureInfo("en-CA");
    Console.WriteLine($"Application ID: {application.Id}");
    Console.WriteLine($"Annual income: {application.AnnualIncome.ToString("C0", cad)}");
    Console.WriteLine($"Monthly expenses: {application.MonthlyExpenses.ToString("C0", cad)}");
    Console.WriteLine($"Deposit: {application.DepositAmount.ToString("C0", cad)}");
    Console.WriteLine($"Property value: {application.PropertyValue.ToString("C0", cad)}");
    Console.WriteLine($"Term: {application.TermYears} years");
    Console.WriteLine($"Submitted: {application.CreatedAt:g}");
}

static async Task ShowErrorAsync(HttpResponseMessage response)
{
    var body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"The API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
    if (!string.IsNullOrWhiteSpace(body)) Console.WriteLine(body);
}

static void ShowConnectionError(HttpRequestException ex) =>
    Console.WriteLine($"Could not reach the API. Check that it is running and the base URL is correct. {ex.Message}");

record CreateApplicationRequest(decimal AnnualIncome, decimal MonthlyExpenses, decimal DepositAmount, decimal PropertyValue, int TermYears);
record MortgageApplicationResponse(Guid Id, decimal AnnualIncome, decimal MonthlyExpenses, decimal DepositAmount, decimal PropertyValue, int TermYears, DateTimeOffset CreatedAt);
record AffordabilityResponse(Guid ApplicationId, decimal LoanToValueRatio, decimal DebtToIncomeRatio, bool Approved, decimal MaximumBorrowingEstimate, string Explanation);
