using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Factory for creating opponent instances based on selected strategy.
/// </summary>
public sealed class ComputerOpponentFactory(IServiceProvider serviceProvider)
    : IComputerOpponentFactory
{
    /// <inheritdoc />
    public IComputerOpponent GetByStrategy(OpponentStrategy opponentStrategy)
    {
        return serviceProvider.GetRequiredKeyedService<IComputerOpponent>(opponentStrategy);
    }

    /// <inheritdoc />
    public IComputerOpponent GetByGame(Game game)
    {
        return GetByStrategy(game.OpponentStrategy);
    }
}
