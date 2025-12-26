using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
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
public record PlayerAttackCommand(GameId GameId, BoardSide BoardSide, string CellCode)
    : IRequest<CellState>;

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
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);

        // 2. Execute domain operation (this will raise domain events)
        var cellState = game.Attack(request.BoardSide, request.CellCode);

        // 3. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        // 4. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);

        // 5. Clear events after dispatching
        game.ClearDomainEvents();

        logger.LogInformation(
            "Player Attack! {GameId} X {CellCode}, result: {CellState}",
            request.GameId.Value,
            request.CellCode,
            cellState
        );

        // 6. Return result
        return cellState;
    }
}
