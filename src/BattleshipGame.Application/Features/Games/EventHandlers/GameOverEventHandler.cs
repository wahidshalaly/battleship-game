using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the GameOverEvent domain event and executes side effects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameOverEventHandler class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
internal class GameOverEventHandler(IBroadcastor broadcastRepository)
    : INotificationHandler<GameOverEvent>
{
    /// <summary>
    /// Handles the GameOverEvent and executes side effects.
    /// </summary>
    /// <param name="notification">The game over event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(GameOverEvent notification, CancellationToken ct)
    {
        var announcement =
            $"Game Over! Game `{notification.GameId.Value}` Winner: {notification.WinnerSide}";
        await broadcastRepository.BroadcastAsync(announcement, ct);
    }
}
