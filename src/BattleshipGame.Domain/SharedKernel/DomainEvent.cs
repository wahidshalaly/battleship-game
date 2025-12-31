namespace BattleshipGame.Domain.SharedKernel;

/// <summary>
/// Base class for domain events. Implements both IDomainEvent (framework-agnostic) and INotification (MediatR).
/// This dual implementation allows domain events to be published through MediatR while maintaining
/// a domain-agnostic interface for domain logic.
/// </summary>
/// <typeparam name="T">The type of the domain event.</typeparam>
public abstract class DomainEvent<T> : IDomainEvent
    where T : class
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Type EventType { get; init; } = typeof(T);
}
