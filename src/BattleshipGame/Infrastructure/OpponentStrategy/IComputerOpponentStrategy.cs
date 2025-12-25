using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

public interface IComputerOpponentStrategy
{
    Task<string> SelectNextAttack(GameId gameId);
}