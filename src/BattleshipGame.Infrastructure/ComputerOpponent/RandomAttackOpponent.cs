using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Random attack strategy that selects cells randomly from available targets on the Player's board.
/// </summary>
public sealed class RandomAttackOpponent : IComputerOpponent
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public OpponentStrategy Strategy => OpponentStrategy.Random;

    /// <inheritdoc />
    public Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken)
    {
        var targets = game.GetNextTargets(BoardSide.Player);

        if (targets.Count == 0)
        {
            throw new InvalidOperationException("No available targets remaining.");
        }

        var index = _random.Next(targets.Count);
        var selectedTarget = targets.ElementAt(index);

        return Task.FromResult(selectedTarget);
    }
}
