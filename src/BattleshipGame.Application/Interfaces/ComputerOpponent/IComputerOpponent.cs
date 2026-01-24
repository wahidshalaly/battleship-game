using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Interfaces.ComputerOpponent;

/// <summary>
/// Defines the contract for computer opponent attack strategies.
/// The opponent always attacks the Player's board.
/// </summary>
public interface IComputerOpponent
{
    /// <summary>
    /// Gets the opponent strategy that this implementation represents.
    /// </summary>
    OpponentStrategy Strategy { get; }

    /// <summary>
    /// Selects the next attack cell for the computer opponent on the Player's board.
    /// </summary>
    /// <param name="game">The game aggregate containing the current game state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cell code for the next attack (e.g., "A1").</returns>
    Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken);
}
