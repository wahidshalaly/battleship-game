using BattleshipGame.SharedKernel;

namespace BattleshipGame.Application.Common.Services;

public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from the given aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root containing domain events.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchEventsAsync<TId>(AggregateRoot<TId> aggregateRoot, CancellationToken cancellationToken)
        where TId : EntityId;

    /// <summary>
    /// Dispatches domain events from multiple aggregate roots.
    /// </summary>
    /// <param name="aggregateRoots">The collection of aggregate roots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchEventsAsync<TId>(IEnumerable<AggregateRoot<TId>> aggregateRoots, CancellationToken cancellationToken)
        where TId : EntityId;
}
