using BattleshipGame.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Common.Services;

/// <summary>
/// Service responsible for dispatching domain events through MediatR.
/// </summary>
/// <remarks>
/// Initializes a new instance of the DomainEventDispatcher class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="mediator">The MediatR mediator instance.</param>
public class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger, IMediator mediator)
    : IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from the given aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root containing domain events.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DispatchEventsAsync<TId>(
        AggregateRoot<TId> aggregateRoot,
        CancellationToken cancellationToken
    )
        where TId : EntityId
    {
        var domainEvents = aggregateRoot.DomainEvents.ToList();

        logger.LogInformation(
            "Dispatching {EventCount} domain events for aggregate {AggregateId}",
            domainEvents.Count,
            aggregateRoot.Id
        );

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                logger.LogDebug(
                    "Publishing domain event {EventType} for aggregate {AggregateId}",
                    domainEvent.GetType().Name,
                    aggregateRoot.Id
                );

                await mediator.Publish(domainEvent, cancellationToken);

                logger.LogDebug(
                    "Successfully published domain event {EventType}",
                    domainEvent.GetType().Name
                );
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish domain event {EventType} for aggregate {AggregateId}",
                    domainEvent.GetType().Name,
                    aggregateRoot.Id
                );

                throw;
            }
        }
    }

    /// <summary>
    /// Dispatches domain events from multiple aggregate roots.
    /// </summary>
    /// <param name="aggregateRoots">The collection of aggregate roots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DispatchEventsAsync<TId>(
        IEnumerable<AggregateRoot<TId>> aggregateRoots,
        CancellationToken cancellationToken
    )
        where TId : EntityId
    {
        foreach (var aggregateRoot in aggregateRoots)
        {
            await DispatchEventsAsync(aggregateRoot, cancellationToken);
        }
    }
}
