using BattleshipGame.SharedKernel;

namespace BattleshipGame.Application.Common.Services;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from the given aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root containing domain events.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchEventsAsync<TId>(AggregateRoot<TId> aggregateRoot, CancellationToken ct)
        where TId : EntityId;

    /// <summary>
    /// Dispatches domain events from multiple aggregate roots.
    /// </summary>
    /// <param name="aggregateRoots">The collection of aggregate roots.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchEventsAsync<TId>(
        IEnumerable<AggregateRoot<TId>> aggregateRoots,
        CancellationToken ct
    )
        where TId : EntityId;
}
