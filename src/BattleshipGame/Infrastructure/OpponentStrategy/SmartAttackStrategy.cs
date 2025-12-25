using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

public class SmartAttackStrategy : IComputerOpponentStrategy
{
    public Task<string> SelectNextAttack(GameId gameId)
    {
        // Hunt/Target algorithm
        // - Hunt mode: Random attacks until hit
        // - Target mode: Attack adjacent cells after hit
        throw new NotImplementedException();
    }
}
