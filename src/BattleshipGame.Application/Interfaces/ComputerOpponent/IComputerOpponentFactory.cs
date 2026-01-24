using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Interfaces.ComputerOpponent;

/// <summary>
/// Factory for creating opponent strategy instances based on strategy type.
/// </summary>
public interface IComputerOpponentFactory
{
    /// <summary>
    /// Gets an opponent strategy instance for the specified opponent type.
    /// </summary>
    /// <param name="opponentStrategy">The type of opponent to create a strategy for.</param>
    /// <returns>The opponent strategy instance.</returns>
    IComputerOpponent GetByStrategy(OpponentStrategy opponentStrategy);

    /// <summary>
    /// Gets an opponent strategy instance based on the game's configured strategy.
    /// </summary>
    /// <param name="game">The game to get the strategy for.</param>
    /// <returns>The opponent strategy instance configured for the game.</returns>
    IComputerOpponent GetByGame(Game game);
}
