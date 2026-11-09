using MortgageApplication.Web.Components;
using MortgageApplication.Web.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<MortgageApplicationApiClient>(client =>
{
    var apiAddress = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5251/";
    client.BaseAddress = new Uri(apiAddress.EndsWith('/') ? apiAddress : apiAddress + "/");
    var apiKey = builder.Configuration["Api:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }
});
var keyDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keyDirectory);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
