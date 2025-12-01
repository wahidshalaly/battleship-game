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
public record AttackCellCommand(GameId GameId, BoardSide BoardSide, string CellCode)
    : IRequest<CellState>;

/// <summary>
/// Handles the AttackCellCommand and demonstrates proper event dispatching.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class AttackCellHandler(
    ILogger<AttackCellHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<AttackCellCommand, CellState>
{
    /// <summary>
    /// Handles the attack cell command.
    /// </summary>
    /// <param name="request">The attack cell command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cell state after attack.</returns>
    public async Task<CellState> Handle(
        AttackCellCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Processing attack on {BoardSide} board at {CellCode} for game {GameId}",
            request.BoardSide,
            request.CellCode,
            request.GameId
        );

        // 1. Load aggregate
        var game =
            await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        // 2. Execute domain operation (this will raise domain events)
        var cellState = game.Attack(request.BoardSide, request.CellCode);

        // 3. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, cancellationToken);

        // 4. Dispatch domain events (THIS IS WHERE SIDE EFFECTS HAPPEN)
        await eventDispatcher.DispatchEventsAsync(game, cancellationToken);

        // 5. Clear events after dispatching
        game.ClearDomainEvents();

        logger.LogInformation(
            "Attack processed successfully for game {GameId}, cell {CellCode}, result: {CellState}",
            request.GameId,
            request.CellCode,
            cellState
        );

        // 6. Return result
        return cellState;
    }
}
