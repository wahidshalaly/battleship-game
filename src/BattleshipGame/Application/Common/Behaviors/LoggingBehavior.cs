using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // JSON serialization options for logging
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.TraceId.ToString() ?? string.Empty;

        // Extract entity context from request
        var requestContext = ExtractEntityContext(request);

        // Build scope with correlation ID, request name, and entity context
        var scopeData = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Request"] = requestName,
        };

        foreach (var kvp in requestContext)
        {
            scopeData[kvp.Key] = kvp.Value;

            // Also add to Activity for distributed tracing
            Activity.Current?.SetTag(ConvertToSnakeCase(kvp.Key), kvp.Value.ToString());
        }

        using var scope = logger.BeginScope(scopeData);

        // Log with entity context
        var contextString = requestContext.Any()
            ? $" [{string.Join(", ", requestContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))}]"
            : string.Empty;

        // Log request with full payload at Debug level
        if (logger.IsEnabled(LogLevel.Debug))
        {
            try
            {
                var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
                logger.LogDebug(
                    "Handling {Request}{Context} with payload: {RequestPayload}",
                    requestName,
                    contextString,
                    requestJson
                );
            }
            catch
            {
                // Fallback if serialization fails
                logger.LogInformation("Handling {Request}{Context}", requestName, contextString);
            }
        }
        else
        {
            logger.LogInformation("Handling {Request}{Context}", requestName, contextString);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            // Extract entity IDs from response
            var responseContext = ExtractEntityContext(response);
            var responseContextString =
                responseContext.Count > 0
                    ? $" [{string.Join(", ", responseContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))}]"
                    : string.Empty;

            // Add response context to Activity
            foreach (var kvp in responseContext)
            {
                Activity.Current?.SetTag(
                    $"response.{ConvertToSnakeCase(kvp.Key)}",
                    kvp.Value.ToString()
                );
            }

            // Log response at Debug level with payload
            if (logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                    logger.LogDebug(
                        "Handled {Request} in {Elapsed}ms{Context} - Response: {ResponsePayload}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        responseContextString,
                        responseJson
                    );
                }
                catch
                {
                    logger.LogInformation(
                        "Handled {Request} in {Elapsed}ms{Context}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        responseContextString
                    );
                }
            }
            else
            {
                logger.LogInformation(
                    "Handled {Request} in {Elapsed}ms{Context}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    responseContextString
                );
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Always log request payload on errors, even if not in Debug mode
            try
            {
                var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
                logger.LogError(
                    ex,
                    "Error handling {Request} after {Elapsed}ms{Context} - Request: {RequestPayload}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    contextString,
                    requestJson
                );
            }
            catch
            {
                // Fallback if serialization fails
                logger.LogError(
                    ex,
                    "Error handling {Request} after {Elapsed}ms{Context}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    contextString
                );
            }

            throw;
        }
    }

    /// <summary>
    /// Converts PascalCase to snake_case for OpenTelemetry conventions.
    /// </summary>
    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = string.Concat(
            input.Select((c, i) => i > 0 && char.IsUpper(c) ? $"_{c}" : c.ToString())
        );

        return result.ToLowerInvariant();
    }

    // Cache property extractors per type to avoid repeated reflection
    private static readonly ConcurrentDictionary<Type, List<PropertyExtractor>> _extractorCache =
        new();

    private record PropertyExtractor(string Name, Func<object, object?> GetValue);

    /// <summary>
    /// Extracts entity IDs and important context from request/response objects.
    /// Uses cached reflection to minimize performance overhead.
    /// </summary>
    private static Dictionary<string, object> ExtractEntityContext(object? obj)
    {
        var context = new Dictionary<string, object>();

        if (obj is null)
            return context;

        var objType = obj.GetType();

        // For Guid responses (like CreatePlayer/CreateGame that return Guid directly)
        if (obj is Guid guid)
        {
            context["EntityId"] = guid;
            return context;
        }

        // Get or build extractors for this type (cached)
        var extractors = _extractorCache.GetOrAdd(objType, BuildExtractors);

        // Execute cached extractors
        foreach (var extractor in extractors)
        {
            try
            {
                var value = extractor.GetValue(obj);
                if (value is not null)
                {
                    context[extractor.Name] = value;
                }
            }
            catch
            {
                // Skip properties that fail to read
            }
        }

        return context;
    }

    /// <summary>
    /// Builds property extractors for a type (called once per type and cached).
    /// </summary>
    private static List<PropertyExtractor> BuildExtractors(Type type)
    {
        var extractors = new List<PropertyExtractor>();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var propName = prop.Name;

            // Look for entity ID properties and important context
            if (
                propName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                || propName.Contains("State")
                || propName.Contains("Side")
                || propName is "Username"
            )
            {
                // Check if this is an EntityId wrapper type that needs unwrapping
                var propType = prop.PropertyType;
                var valueProperty = propType.GetProperty("Value");

                if (valueProperty is not null && propType.Name.EndsWith("Id"))
                {
                    // Create extractor that unwraps EntityId -> Guid
                    extractors.Add(
                        new PropertyExtractor(
                            propName,
                            obj =>
                            {
                                var wrapper = prop.GetValue(obj);
                                return wrapper is not null ? valueProperty.GetValue(wrapper) : null;
                            }
                        )
                    );
                }
                else
                {
                    // Create direct property extractor
                    extractors.Add(new PropertyExtractor(propName, prop.GetValue));
                }
            }
        }

        return extractors;
    }
}
