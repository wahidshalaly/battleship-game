using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

public class RandomAttackStrategy(IGameRepository gameRepository) : IComputerOpponentStrategy
{
    private readonly Random _random = new();

    public async Task<string> SelectNextAttack(GameId gameId)
    {
        var game = await gameRepository.GetByIdOrThrowAsync(gameId, CancellationToken.None);
        var nextTargets = game.GetNextTargets(BoardSide.Player);
        if (nextTargets.Count == 0)
        {
            throw new InvalidOperationException("No available targets remaining.");
        }

        // Pick random unattacked cell
        var idx = _random.Next(nextTargets.Count);
        return nextTargets.ElementAt(idx);
    }
}
