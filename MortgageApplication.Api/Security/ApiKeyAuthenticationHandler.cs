using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MortgageApplicationApi.Security;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    private const string HeaderName = "X-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out Microsoft.Extensions.Primitives.StringValues providedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        byte[] expectedBytes = Encoding.UTF8.GetBytes(Options.ApiKey ?? string.Empty);
        byte[] providedBytes = Encoding.UTF8.GetBytes(providedKey.ToString());
        if (expectedBytes.Length == 0 ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
        {
            Logger.LogWarning("Rejected request with an invalid API key.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        ClaimsIdentity identity = new(
            [new Claim(ClaimTypes.Name, "api-client")], SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "ApiKey";
        await Response.WriteAsJsonAsync(new
        {
            title = "Authentication required",
            detail = "A valid API key must be supplied."
        });
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsJsonAsync(new
        {
            title = "Access denied",
            detail = "This client is not allowed to access the requested resource."
        });
    }
}
