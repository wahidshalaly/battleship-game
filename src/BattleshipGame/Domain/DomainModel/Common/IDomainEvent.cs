namespace BattleshipGame.Domain.DomainModel.Common;

public interface IDomainEvent
{
    Guid EventId { get; init; }
    DateTime OccurredOn { get; init; }
    Type EventType { get; init; }
}
