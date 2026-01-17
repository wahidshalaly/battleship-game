using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

/// <summary>
/// Random attack strategy that selects cells randomly from available targets on the Player's board.
/// </summary>
public sealed class RandomAttackStrategy : IComputerOpponentStrategy
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public OpponentStrategyType StrategyType => OpponentStrategyType.Random;

    /// <inheritdoc />
    public Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken)
    {
        var availableTargets = game.GetNextTargets(BoardSide.Player);

        if (availableTargets.Count == 0)
        {
            throw new InvalidOperationException("No available targets remaining.");
        }

        var index = _random.Next(availableTargets.Count);
        var selectedCell = availableTargets.ElementAt(index);

        return Task.FromResult(selectedCell);
    }
}
