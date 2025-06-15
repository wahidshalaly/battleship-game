using System;

namespace BattleshipGame.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; init; }
    DateTime OccurredOn { get; init; }
    Type EventType { get; init; }
}
