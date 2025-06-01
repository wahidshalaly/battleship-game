using System;

namespace BattleshipGame.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; init; }
    DateTime OccurredOn { get; init; }
    Type EventType { get; init; }
}
