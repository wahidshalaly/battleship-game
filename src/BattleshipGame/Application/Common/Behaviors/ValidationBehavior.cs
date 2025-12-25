using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        if (!validators.Any())
        {
            return await next(ct);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, ct);
            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        if (failures.Count != 0)
        {
            logger.LogWarning(
                "Validation failed for {RequestType}: {Failures}",
                typeof(TRequest).Name,
                failures
            );
            throw new ValidationException(failures);
        }

        return await next(ct);
    }
}
