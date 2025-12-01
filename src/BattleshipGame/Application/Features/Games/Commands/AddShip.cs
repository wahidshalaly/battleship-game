using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Represents a command to add a ship to a game board.
/// </summary>
/// <param name="GameId">The unique identifier of the game to which the ship is being added. Cannot be null.</param>
/// <param name="BoardSide">The side of the board where the ship will be placed. Must be a valid board side.</param>
/// <param name="Kind">The type of ship to be added (e.g., battleship, cruiser). Must be a valid ship kind.</param>
/// <param name="Orientation">The orientation of the ship on the board (e.g., horizontal, vertical). Must be a valid orientation.</param>
/// <param name="BowCode">The code representing the position of the ship's bow on the board. Cannot be null or empty.</param>
public record AddShipCommand(
    GameId GameId,
    BoardSide BoardSide,
    ShipKind Kind,
    ShipOrientation Orientation,
    string BowCode
) : IRequest<Guid>;

internal class AddShipHandler(
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<AddShipCommand, Guid>
{
    public async Task<Guid> Handle(AddShipCommand request, CancellationToken cancellationToken)
    {
        // Load the game aggregate
        var game =
            await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        // Add the ship to the specified board side
        var shipId = game.AddShip(
            request.BoardSide,
            request.Kind,
            request.Orientation,
            request.BowCode
        );

        // Save the updated game aggregate
        await gameRepository.SaveAsync(game, cancellationToken);

        // Dispatch any domain events raised by the AddShip method
        await eventDispatcher.DispatchEventsAsync(game, cancellationToken);

        return shipId;
    }
}
