using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.OpponentStrategy;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to perform an opponent's attack in a battleship game.
/// </summary>
/// <param name="GameId">The game identifier.</param>
/// <returns>The result of the opponent's attack.</returns>
public record OpponentAttackCommand(GameId GameId) : IRequest<CellState>;

/// <summary>
/// Handles the OpponentAttackCommand and demonstrates proper event dispatching.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="opponentStrategy">The computer opponent strategy service.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class OpponentAttackHandler(
    ILogger<OpponentAttackHandler> logger,
    IGameRepository gameRepository,
    IComputerOpponentStrategy opponentStrategy,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<OpponentAttackCommand, CellState>
{
    /// <summary>
    /// Handles the opponent attack command.
    /// </summary>
    /// <param name="request">The opponent attack command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the opponent's attack.</returns>
    public async Task<CellState> Handle(OpponentAttackCommand request, CancellationToken ct)
    {
        // 1. Load aggregate
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);

        // 2. Select target cell and perform attack
        var targetCell = await opponentStrategy.SelectNextAttack(request.GameId);
        var cellState = game.Attack(BoardSide.Player, targetCell);

        // 3. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        // 4. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);

        // 5. Clear events after dispatching
        game.ClearDomainEvents();

        logger.LogInformation(
            "Opponent Attack! {GameId} X {CellCode}, result: {CellState}",
            request.GameId.Value,
            targetCell,
            cellState
        );

        // 6. Return result
        return cellState;
    }
}
