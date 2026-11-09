using Microsoft.AspNetCore.Diagnostics;

namespace MortgageApplicationApi.ExceptionHandling;

public class SafeApiExceptionHandler(ILogger<SafeApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled API exception for request {RequestId}.", httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            title = "Request failed",
            detail = "The server could not complete the request."
        }, cancellationToken);

        return true;
    }
}
