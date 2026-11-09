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
        var handler = new SafeApiExceptionHandler(NullLogger<SafeApiExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var exception = new InvalidOperationException("private file path and stack trace");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        body.Position = 0;
        var response = await new StreamReader(body, Encoding.UTF8).ReadToEndAsync();
        Assert.Contains("The server could not complete the request.", response);
        Assert.DoesNotContain("private file path", response);
        Assert.DoesNotContain("InvalidOperationException", response);
        Assert.DoesNotContain("500", response);
    }
}
