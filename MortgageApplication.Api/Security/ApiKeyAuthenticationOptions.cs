using Microsoft.AspNetCore.Authentication;

namespace MortgageApplicationApi.Security;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string? ApiKey { get; set; }
}
