using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;

namespace BattleshipGame.Application.Features.Games.EventHandlers;

/// <summary>
/// Handles the ShipSunkEvent domain event and executes side effects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ShipSunkEventHandler class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
internal class ShipSunkEventHandler(IBroadcastor broadcastRepository)
    : INotificationHandler<ShipSunkEvent>
{
    /// <summary>
    /// Handles the ShipSunkEvent and executes side effects.
    /// This is mainly an example of how to handle domain events for side effects for now.
    /// </summary>
    /// <param name="notification">The under attack event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(ShipSunkEvent notification, CancellationToken ct)
    {
        var announcement =
            $"Ship Sunk! Game `{notification.GameId.Value}` Ship `{notification.ShipId.Value}` Side: {notification.AttackedSide}";
        await broadcastRepository.BroadcastAsync(announcement, ct);
    }
}
