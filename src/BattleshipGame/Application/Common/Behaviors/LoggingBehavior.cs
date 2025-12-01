using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        using var scope = logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Request"] = requestName,
            }
        );
        logger.LogInformation(
            "Handling {Request} (corr={CorrelationId})",
            requestName,
            correlationId
        );

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            logger.LogInformation(
                "Handled {Request} in {Elapsed} ms (corr={CorrelationId})",
                requestName,
                sw.ElapsedMilliseconds,
                correlationId
            );
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "Error handling {Request} after {Elapsed} ms (corr={CorrelationId})",
                requestName,
                sw.ElapsedMilliseconds,
                correlationId
            );
            throw;
        }
    }
}
