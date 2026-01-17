using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Contracts.OpponentStrategy;

/// <summary>
/// Defines the contract for computer opponent attack strategies.
/// The opponent always attacks the Player's board.
/// </summary>
public interface IComputerOpponentStrategy
{
    /// <summary>
    /// Gets the strategy type this implementation represents.
    /// </summary>
    OpponentStrategyType StrategyType { get; }

    /// <summary>
    /// Selects the next attack cell for the computer opponent on the Player's board.
    /// </summary>
    /// <param name="game">The game aggregate containing the current game state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cell code for the next attack (e.g., "A1").</returns>
    Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken);
}
