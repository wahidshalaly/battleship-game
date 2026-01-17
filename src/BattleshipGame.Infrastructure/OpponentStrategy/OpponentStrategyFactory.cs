using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

/// <summary>
/// Factory for creating opponent strategy instances based on strategy type.
/// Uses keyed services for strategy resolution.
/// </summary>
public sealed class OpponentStrategyFactory(IServiceProvider serviceProvider)
    : IOpponentStrategyFactory
{
    /// <inheritdoc />
    public IComputerOpponentStrategy GetStrategy(OpponentStrategyType strategyType)
    {
        return serviceProvider.GetRequiredKeyedService<IComputerOpponentStrategy>(strategyType);
    }

    /// <inheritdoc />
    public IComputerOpponentStrategy GetStrategy(Game game)
    {
        return GetStrategy(game.OpponentStrategyType);
    }
}
