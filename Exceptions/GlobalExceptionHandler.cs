using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web_bite_server.Exceptions;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Detail = exception.Message
        };
        switch (exception)
        {
            case NotFoundException:
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = exception.GetType().Name;
                break;
            case BadHttpRequestException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = exception.GetType().Name;
                break;
            default:
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}