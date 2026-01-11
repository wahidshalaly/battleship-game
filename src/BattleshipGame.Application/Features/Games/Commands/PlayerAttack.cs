using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to attack a cell in a battleship game.
/// </summary>
/// <param name="GameId">The game identifier.</param>
/// <param name="BoardSide">The board side to attack.</param>
/// <param name="CellCode">The cell code to attack (e.g., "A1", "B5").</param>
public record PlayerAttackCommand(GameId GameId, string CellCode) : IRequest<CellState>;

/// <summary>
/// Handles the AttackCommand and demonstrates proper event dispatching.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class PlayerAttackHandler(
    ILogger<PlayerAttackHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<PlayerAttackCommand, CellState>
{
    /// <summary>
    /// Handles the attack cell command.
    /// </summary>
    /// <param name="request">The attack cell command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The cell state after attack.</returns>
    public async Task<CellState> Handle(PlayerAttackCommand request, CancellationToken ct)
    {
        // 1. Load aggregate
        var game = await gameRepository.GetByIdOrThrowAsync(request.GameId, ct);

        // 2. Start gameplay if not already started
        if (game.TargetSide == BoardSide.None && game.State == GameState.Ready)
        {
            game.StartGameplay();
            logger.LogInformation("Gameplay started. {@Payload}", new { GameId = game.Id.Value });
        }

        // 3. Perform attack
        var cellState = game.Attack(BoardSide.Opponent, request.CellCode);

        // 4. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        logger.LogInformation(
            "Player Attack! Game `{GameId}` X {CellCode}, Outcome: {CellState}",
            request.GameId.Value,
            request.CellCode,
            cellState
        );

        // 5. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);

        return cellState;
    }
}
