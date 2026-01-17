using BattleshipGame.Application.Common;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

/// <summary>
/// Analyzes game state to provide AI with decision context.
/// Extracts hit/miss history, remaining ships, and board patterns.
/// </summary>
public class GameStateAnalyzer(IGameRepository gameRepository)
{
    /// <summary>
    /// Analyzes the current game state for AI decision-making.
    /// </summary>
    public async Task<GameStateContext> AnalyzeGameStateAsync(
        GameId gameId,
        CancellationToken cancellationToken
    )
    {
        var game = await gameRepository.GetByIdOrThrowAsync(gameId, cancellationToken);
        var targets = game.GetNextTargets(BoardSide.Player).ToList();
        var hits = game.GetHits(BoardSide.Player).ToList();
        var misseds = game.GetMisseds(BoardSide.Player).ToList();

        return new GameStateContext
        {
            BoardSize = game.BoardSize,
            NextTargets = targets,
            Hits = hits,
            Misseds = misseds,
            ShipsSunk = 0, // Will track sunk ships in future enhancement
        };
    }
}
