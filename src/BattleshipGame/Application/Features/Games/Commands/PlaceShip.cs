using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Represents a command to place a ship on a game board.
/// </summary>
/// <param name="GameId">The unique identifier of the game to which the ship is being placed. Cannot be null.</param>
/// <param name="BoardSide">The side of the board where the ship will be placed. Must be a valid board side.</param>
/// <param name="Kind">The type of ship to be placed (e.g., battleship, cruiser). Must be a valid ship kind.</param>
/// <param name="Orientation">The orientation of the ship on the board (e.g., horizontal, vertical). Must be a valid orientation.</param>
/// <param name="BowCode">The code representing the position of the ship's bow on the board. Cannot be null or empty.</param>
public record PlaceShipCommand(
    GameId GameId,
    BoardSide BoardSide,
    ShipKind Kind,
    ShipOrientation Orientation,
    string BowCode
) : IRequest<Guid>;

internal class PlaceShipHandler(
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<PlaceShipCommand, Guid>
{
    public async Task<Guid> Handle(PlaceShipCommand request, CancellationToken ct)
    {
        // Load the game aggregate
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);

        // Place the ship to the specified board side
        var shipId = game.PlaceShip(
            request.BoardSide,
            request.Kind,
            request.Orientation,
            request.BowCode
        );

        // Save the updated game aggregate
        await gameRepository.SaveAsync(game, ct);

        // Dispatch any domain events raised by the PlaceShip method
        await eventDispatcher.DispatchEventsAsync(game, ct);

        return shipId;
    }
}
