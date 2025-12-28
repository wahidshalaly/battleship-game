using System.Diagnostics;

namespace BattleshipGame.WebAPI.Middleware;

/// <summary>
/// Middleware to log all HTTP requests and responses with timing and status codes.
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            var logLevel = statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information,
            };

            logger.Log(
                logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                requestMethod,
                requestPath,
                statusCode,
                elapsed
            );

            // Add additional context for error responses
            if (statusCode >= 400)
            {
                logger.LogWarning(
                    "Request failed: {Method} {Path} | Status: {StatusCode} | QueryString: {QueryString} | RemoteIP: {RemoteIP}",
                    requestMethod,
                    requestPath,
                    statusCode,
                    context.Request.QueryString,
                    context.Connection.RemoteIpAddress
                );
            }
        }
    }
}
