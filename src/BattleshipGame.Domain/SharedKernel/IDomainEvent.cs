using MediatR;

namespace BattleshipGame.Domain.SharedKernel;

/// <summary>
/// Represents a domain event in the system.
/// Inherits from MediatR's INotification to enable event publishing through MediatR.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; init; }
    DateTime OccurredOn { get; init; }
    Type EventType { get; init; }
}
