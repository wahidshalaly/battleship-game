using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the UnderAttackEvent domain event and executes side effects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UnderAttackEventHandler class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
internal class UnderAttackEventHandler(IBroadcastRepository broadcastRepository)
    : INotificationHandler<UnderAttackEvent>
{
    /// <summary>
    /// Handles the UnderAttackEvent and executes side effects.
    /// This is mainly an example of how to handle domain events for side effects for now.
    /// </summary>
    /// <param name="notification">The under attack event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(UnderAttackEvent notification, CancellationToken ct)
    {
        var announcement =
            $"Attack! Game `{notification.GameId.Value}` Side: {notification.BoardSide} x {notification.CellCode}: {notification.CellState}";
        await broadcastRepository.AnnounceAsync(announcement, ct);
    }
}
