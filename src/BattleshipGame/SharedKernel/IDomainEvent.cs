using MediatR;

namespace BattleshipGame.SharedKernel;

public interface IDomainEvent : INotification
{
    Guid EventId { get; init; }
    DateTime OccurredOn { get; init; }
    Type EventType { get; init; }
}
