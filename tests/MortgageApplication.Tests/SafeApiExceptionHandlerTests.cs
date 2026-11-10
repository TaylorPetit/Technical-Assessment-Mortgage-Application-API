using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MortgageApplicationApi.ExceptionHandling;

namespace MortgageApplication.Tests;

public class SafeApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_LogsExceptionAndReturnsGenericResponse()
    {
        SafeApiExceptionHandler handler = new(NullLogger<SafeApiExceptionHandler>.Instance);
        DefaultHttpContext context = new();
        await using MemoryStream body = new();
        context.Response.Body = body;
        InvalidOperationException exception = new("private file path and stack trace");

        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        body.Position = 0;
        string response = await new StreamReader(body, Encoding.UTF8).ReadToEndAsync();
        Assert.Contains("The server could not complete the request.", response);
        Assert.DoesNotContain("private file path", response);
        Assert.DoesNotContain("InvalidOperationException", response);
        Assert.DoesNotContain("500", response);
    }
}
