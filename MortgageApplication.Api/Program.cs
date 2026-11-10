using MortgageApplicationApi.Repositories;
using MortgageApplicationApi.Security;
using MortgageApplicationApi.Services;
using MortgageApplicationApi.ExceptionHandling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SafeApiExceptionHandler>();
string? apiKey = builder.Configuration["Authentication:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException("Configure the Authentication:ApiKey setting before starting the API.");
}

builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        options => options.ApiKey = apiKey);
builder.Services.AddAuthorization();

string applicationsFile = builder.Configuration["Storage:ApplicationsFile"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "mortgage-applications.json");

builder.Services.AddSingleton<IMortgageApplicationRepository>(_ => new JsonMortgageApplicationRepository(applicationsFile));
builder.Services.AddSingleton<IAffordabilityAssessor, AffordabilityAssessor>();
builder.Services.AddScoped<MortgageApplicationService>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
