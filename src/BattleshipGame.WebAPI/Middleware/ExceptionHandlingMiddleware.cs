using System.Net;
using System.Text.Json;
using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Middleware;

/// <summary>
/// Middleware to handle exceptions globally and return ProblemDetails responses.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var problemDetails = ToProblemDetails(context, ex);
            Log(ex, problemDetails.Status ?? StatusCodes.Status500InternalServerError);
            await WriteAsync(context, problemDetails);
        }
    }

    /// <summary>
    /// Logs server errors at Error level with the exception. Client errors (4xx) are expected
    /// outcomes such as a wrong password or a conflict, so they are logged at Warning level
    /// without a stack trace.
    /// </summary>
    /// <param name="exception">The exception that was handled.</param>
    /// <param name="status">The HTTP status code returned for it.</param>
    private void Log(Exception exception, int status)
    {
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unhandled exception occurred: {Message}",
                exception.Message
            );
            return;
        }

        logger.LogWarning(
            "Request rejected with {StatusCode} ({ExceptionType}): {Message}",
            status,
            exception.GetType().Name,
            exception.Message
        );
    }

    private static ProblemDetails ToProblemDetails(HttpContext context, Exception exception) =>
        exception switch
        {
            GameNotFoundException gnfEx => new ProblemDetails
            {
                Title = "Game Not Found",
                Detail = gnfEx.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = context.Request.Path,
            },
            PlayerNotFoundException pnfEx => new ProblemDetails
            {
                Title = "Player Not Found",
                Detail = pnfEx.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = context.Request.Path,
            },
            ForbiddenAccessException faEx => new ProblemDetails
            {
                Title = "Forbidden",
                Detail = faEx.Message,
                Status = StatusCodes.Status403Forbidden,
                Instance = context.Request.Path,
            },
            IdentityConflictException icEx => new ProblemDetails
            {
                Title = "Conflict",
                Detail = icEx.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = context.Request.Path,
            },
            InvalidCredentialsException => new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid username or password.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path,
            },
            PlayerHasActiveGameException phagEx => new ProblemDetails
            {
                Title = "Player Already In Game",
                Detail = phagEx.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = context.Request.Path,
                Extensions = { ["activeGameId"] = phagEx.ActiveGameId },
            },
            InvalidOperationException ioEx => new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ioEx.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path,
            },
            ArgumentException argEx => new ProblemDetails
            {
                Title = "Invalid Argument",
                Detail = argEx.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path,
            },
            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path,
            },
        };

    private static Task WriteAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode =
            problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(
            problemDetails,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return context.Response.WriteAsync(json);
    }
}
