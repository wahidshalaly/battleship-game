using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BattleshipGame.WebAPI.Filters;

/// <summary>
/// Validates each action argument against its registered FluentValidation
/// <see cref="IValidator{T}"/> (if any) before the action executes. Failures are added to
/// <see cref="ModelStateDictionary"/> and, when invalid, the configured
/// <see cref="ApiBehaviorOptions.InvalidModelStateResponseFactory"/> produces the response.
/// </summary>
public sealed class FluentValidationActionFilter(
    IServiceProvider serviceProvider,
    IOptions<ApiBehaviorOptions> apiBehaviorOptions
) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted
            );

            if (result.IsValid)
                continue;

            foreach (var failure in result.Errors)
            {
                context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = apiBehaviorOptions.Value.InvalidModelStateResponseFactory(context);
            return;
        }

        await next();
    }
}
