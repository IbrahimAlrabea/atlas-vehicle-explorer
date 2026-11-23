
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace VehicleExplorer.Api.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The client disconnected or cancelled the request.
        if (exception is OperationCanceledException &&
            context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request cancelled by client: {Path}",
                context.Request.Path);

            // Let the exception-handling middleware handle cancellation.
            return false;
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "Cannot write an error response because it has already started. Path: {Path}",
                context.Request.Path);

            return false;
        }

        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest,
                 "Invalid request parameters"),

            HttpRequestException =>
                (StatusCodes.Status502BadGateway,
                 "Vehicle data provider unavailable"),

            JsonException =>
                (StatusCodes.Status502BadGateway,
                 "Invalid vehicle data provider response"),

            OperationCanceledException =>
                (StatusCodes.Status504GatewayTimeout,
                 "Vehicle data provider timed out"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Unexpected server error")
        };

        _logger.LogError(
            exception,
            "Request failed. Path: {Path}, StatusCode: {StatusCode}",
            context.Request.Path,
            statusCode);

        context.Response.StatusCode = statusCode;

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            instance: context.Request.Path
        ).ExecuteAsync(context);

        return true;
    }
}
