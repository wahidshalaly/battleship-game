using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to attack a cell in a battleship game.
/// </summary>
/// <param name="GameId">The game identifier.</param>
/// <param name="CellCode">The cell code to attack (e.g., "A1", "B5").</param>
public record PlayerAttackCommand(GameId GameId, string CellCode) : IRequest<AttackResult>;

/// <summary>
/// Handles the PlayerAttackCommand by performing an attack on the opponent's board.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class PlayerAttackHandler(
    ILogger<PlayerAttackHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<PlayerAttackCommand, AttackResult>
{
    /// <summary>
    /// Handles the player attack command.
    /// </summary>
    /// <param name="request">The player attack command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The attack result including cell state and game state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when game is not in Started state.</exception>
    public async Task<AttackResult> Handle(PlayerAttackCommand request, CancellationToken ct)
    {
        // 1. Load aggregate
        var game = await gameRepository.GetByIdOrThrowAsync(request.GameId, ct);

        // 2. Validate game is in Started state
        if (game.State != GameState.Started)
        {
            throw new GameNotStartedException(game.Id, game.State);
        }

        // 3. Perform attack
        var cellState = game.Attack(BoardSide.Opponent, request.CellCode);

        // 4. Check if a ship was sunk by inspecting domain events
        var shipSunkEvent = game
            .DomainEvents.OfType<ShipSunkEvent>()
            .FirstOrDefault(e => e.AttackedSide == BoardSide.Opponent);

        ShipKind? sunkShip = null;
        if (shipSunkEvent is not null)
        {
            sunkShip = game.GetShipKind(BoardSide.Opponent, shipSunkEvent.ShipId);
        }

        // 5. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        logger.LogInformation(
            "Player Attack! Game `{GameId}` X {CellCode}, Outcome: {CellState}, Ship Sunk: {SunkShip}",
            request.GameId.Value,
            request.CellCode,
            cellState,
            sunkShip?.ToString() ?? "None"
        );

        // 6. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);

        // 7. Return attack result
        return new AttackResult(
            TargetCell: request.CellCode,
            CellState: cellState,
            GameState: game.State,
            WinnerSide: game.WinnerSide,
            SunkShip: sunkShip,
            ShipSize: sunkShip?.ToSize()
        );
    }
}
