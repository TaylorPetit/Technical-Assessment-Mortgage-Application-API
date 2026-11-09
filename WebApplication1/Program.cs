using WebApplication1.Repositories;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var applicationsFile = builder.Configuration["Storage:ApplicationsFile"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "mortgage-applications.json");

builder.Services.AddSingleton<IMortgageApplicationRepository>(_ => new JsonMortgageApplicationRepository(applicationsFile));
builder.Services.AddSingleton<AffordabilityAssessor>();
builder.Services.AddScoped<MortgageApplicationService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
