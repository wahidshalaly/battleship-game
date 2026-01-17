using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to perform an opponent's attack in a battleship game.
/// </summary>
/// <param name="GameId">The game identifier.</param>
/// <returns>The result of the opponent's attack.</returns>
public record OpponentAttackCommand(GameId GameId) : IRequest<AttackResult>;

/// <summary>
/// Handles the OpponentAttackCommand and demonstrates proper event dispatching.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="strategyFactory">The opponent strategy factory.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class OpponentAttackHandler(
    ILogger<OpponentAttackHandler> logger,
    IGameRepository gameRepository,
    IOpponentStrategyFactory strategyFactory,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<OpponentAttackCommand, AttackResult>
{
    /// <summary>
    /// Handles the opponent attack command.
    /// </summary>
    /// <param name="request">The opponent attack command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The attack result including cell state and game state.</returns>
    public async Task<AttackResult> Handle(OpponentAttackCommand request, CancellationToken ct)
    {
        // 1. Load aggregate
        var game = await gameRepository.GetByIdOrThrowAsync(request.GameId, ct);

        // 2. Get strategy based on game configuration and select target cell
        var strategy = strategyFactory.GetStrategy(game);
        var targetCell = await strategy.SelectNextAttackAsync(game, ct);
        var cellState = game.Attack(BoardSide.Player, targetCell);

        // 3. Check if a ship was sunk by inspecting domain events
        var shipSunkEvent = game
            .DomainEvents.OfType<ShipSunkEvent>()
            .FirstOrDefault(e => e.AttackedSide == BoardSide.Player);

        ShipKind? sunkShip = null;
        if (shipSunkEvent is not null)
        {
            sunkShip = game.GetShipKind(BoardSide.Player, shipSunkEvent.ShipId);
        }

        // 4. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        logger.LogInformation(
            "Opponent Attack! Game `{GameId}` X {CellCode}, Outcome: {CellState}, Ship Sunk: {SunkShip}",
            request.GameId.Value,
            targetCell,
            cellState,
            sunkShip?.ToString() ?? "None"
        );

        // 5. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);

        // 6. Return attack result
        return new AttackResult(
            TargetCell: targetCell,
            CellState: cellState,
            GameState: game.State,
            WinnerSide: game.WinnerSide,
            SunkShip: sunkShip,
            ShipSize: sunkShip?.ToSize()
        );
    }
}
