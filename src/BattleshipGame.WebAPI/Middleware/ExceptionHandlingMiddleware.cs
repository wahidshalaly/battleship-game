using System.Net;
using System.Text.Json;
using BattleshipGame.Application.Exceptions;
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
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
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
            PlayerIsInActiveException piaEx => new ProblemDetails
            {
                Title = "Player Already In Game",
                Detail = piaEx.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path,
            },
            PlayerIsNotInActiveException pniaEx => new ProblemDetails
            {
                Title = "Player Not In Game",
                Detail = pniaEx.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path,
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
