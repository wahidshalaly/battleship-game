using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Contracts.OpponentStrategy;

/// <summary>
/// Factory for creating opponent strategy instances based on strategy type.
/// </summary>
public interface IOpponentStrategyFactory
{
    /// <summary>
    /// Gets an opponent strategy instance for the specified strategy type.
    /// </summary>
    /// <param name="strategyType">The type of strategy to create.</param>
    /// <returns>The opponent strategy instance.</returns>
    IComputerOpponentStrategy GetStrategy(OpponentStrategyType strategyType);

    /// <summary>
    /// Gets an opponent strategy instance based on the game's configured strategy.
    /// </summary>
    /// <param name="game">The game to get the strategy for.</param>
    /// <returns>The opponent strategy instance configured for the game.</returns>
    IComputerOpponentStrategy GetStrategy(Game game);
}
