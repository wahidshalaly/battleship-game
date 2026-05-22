using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the BoardReadyEvent domain event and executes side effects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the BoardReadyEventHandler class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
internal class BoardReadyEventHandler(IBroadcastor broadcastRepository)
    : INotificationHandler<BoardReadyEvent>
{
    /// <summary>
    /// Handles the BoardReadyEvent and executes side effects.
    /// </summary>
    /// <param name="notification">The game over event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(BoardReadyEvent notification, CancellationToken ct)
    {
        var announcement =
            $"Board Ready! Game `{notification.GameId.Value}` Side: {notification.BoardSide}";
        await broadcastRepository.BroadcastAsync(announcement, ct);
    }
}
