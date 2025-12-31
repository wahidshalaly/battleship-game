using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the GameStartedEvent domain event and executes side effects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameStartedEventHandler class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
internal class GameStartedEventHandler(IBroadcastRepository broadcastRepository)
    : INotificationHandler<GameStartedEvent>
{
    /// <summary>
    /// Handles the GameStartedEvent and executes side effects.
    /// This is mainly an example of how to handle domain events for side effects for now.
    /// </summary>
    /// <param name="notification">The under attack event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(GameStartedEvent notification, CancellationToken ct)
    {
        var announcement = $"Started! Game `{notification.GameId.Value}`";
        await broadcastRepository.AnnounceAsync(announcement, ct);
    }
}
