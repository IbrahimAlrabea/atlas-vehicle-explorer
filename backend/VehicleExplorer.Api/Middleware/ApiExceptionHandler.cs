using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace VehicleExplorer.Api.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            return false;

        var (status, title) = exception switch
        {
            ArgumentException => (400, "Invalid request"),
            HttpRequestException => (502, "Vehicle data provider unavailable"),
            OperationCanceledException => (504, "Vehicle data provider timed out"),
            JsonException => (502, "Invalid vehicle data provider response"),
            _ => (500, "Unexpected server error")
        };
        context.Response.StatusCode = status;
        await Results.Problem(statusCode: status, title: title).ExecuteAsync(context);
        return true;
    }
}
