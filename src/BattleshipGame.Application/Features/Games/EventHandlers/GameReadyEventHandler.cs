using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the GameReadyEvent domain event and executes side effects.
/// </summary>
/// <param name="logger">The logger instance.</param>
internal class GameReadyEventHandler(IBroadcastor broadcastRepository)
    : INotificationHandler<GameReadyEvent>
{
    /// <summary>
    /// Handles the GameReadyEvent and executes side effects.
    /// </summary>
    /// <param name="notification">The boards ready event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(GameReadyEvent notification, CancellationToken ct)
    {
        var announcement = $"Ready! Game `{notification.GameId.Value}`";
        await broadcastRepository.BroadcastAsync(announcement, ct);
    }
}
