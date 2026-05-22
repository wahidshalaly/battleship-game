using BattleshipGame.Application.Interfaces.Persistence;
using MediatR;

namespace BattleshipGame.Application.Common.Behaviors;

/// <summary>
/// Commits the unit of work after every request handler succeeds.
/// Queries produce no tracked changes, so CommitAsync is a no-op for them.
/// Commands that write to multiple aggregates are committed atomically.
/// </summary>
internal class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        var response = await next(ct);
        await unitOfWork.CommitAsync(ct);
        return response;
    }
}
