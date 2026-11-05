using System.Text.Json;

using Microsoft.AspNetCore.Diagnostics;

using Microsoft.EntityFrameworkCore;

using Manbaa.Api.Extensions;

using Manbaa.Application.Common;

using Manbaa.Application.Exceptions;

namespace Manbaa.Api.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        var result = Map(exception);

        if (result.Status == ResultStatus.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        else
        {
            logger.LogWarning("Request {Method} {Path} failed with {Status}: {Message}", httpContext.Request.Method, httpContext.Request.Path, result.Status, exception.Message);
        }

        await httpContext.Response.WriteResultAsync(result, cancellationToken);

        return true;
    }

    private Result Map(Exception exception) => exception switch
    {
        AppException appException => Result.Failure(appException.Status, appException.Message),

        UnauthorizedAccessException unauthorized => Result.Unauthorized(unauthorized.Message),

        DbUpdateConcurrencyException => Result.Conflict("The record was modified by someone else. Reload and try again."),

        DbUpdateException => Result.Conflict("The operation conflicts with existing data."),

        BadHttpRequestException or JsonException => Result.BadRequest("The request body is invalid."),

        _ => Result.InternalServerError(environment.IsDevelopment() ? exception.Message : null)
    };
}
