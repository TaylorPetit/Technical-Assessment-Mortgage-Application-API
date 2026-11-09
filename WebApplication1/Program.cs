using WebApplication1.Repositories;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var applicationsFile = builder.Configuration["Storage:ApplicationsFile"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "mortgage-applications.json");
builder.Services.AddSingleton<IMortgageApplicationRepository>(
    _ => new JsonMortgageApplicationRepository(applicationsFile));
builder.Services.AddSingleton<AffordabilityAssessor>();
builder.Services.AddScoped<MortgageApplicationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
