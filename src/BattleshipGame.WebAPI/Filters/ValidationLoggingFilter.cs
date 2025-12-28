using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BattleshipGame.WebAPI.Filters;

/// <summary>
/// Logs validation errors and model binding failures that result in BadRequest responses.
/// </summary>
public sealed class ValidationLoggingFilter(ILogger<ValidationLoggingFilter> logger)
    : IActionFilter,
        IResultFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check for model binding errors
        if (!context.ModelState.IsValid)
        {
            var errors = context
                .ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            logger.LogWarning(
                "Model validation failed for {ActionName}. Errors: {@ValidationErrors}",
                context.ActionDescriptor.DisplayName,
                errors
            );
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing needed here
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Log BadRequest results with validation details
        if (context.Result is BadRequestObjectResult badRequest)
        {
            logger.LogWarning(
                "BadRequest returned for {ActionName}. Details: {@ValidationProblem}",
                context.ActionDescriptor.DisplayName,
                badRequest.Value
            );
        }
        else if (context.Result is UnprocessableEntityObjectResult unprocessableEntity)
        {
            logger.LogWarning(
                "UnprocessableEntity returned for {ActionName}. Details: {@ValidationProblem}",
                context.ActionDescriptor.DisplayName,
                unprocessableEntity.Value
            );
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Nothing needed here
    }
}
