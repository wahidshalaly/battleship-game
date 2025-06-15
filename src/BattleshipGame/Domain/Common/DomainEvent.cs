using System;

namespace BattleshipGame.Domain.Common;

public abstract class DomainEvent<T> : IDomainEvent
    where T : class
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Type EventType { get; init; } = typeof(T);
}
