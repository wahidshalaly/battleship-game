using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Contracts.OpponentStrategy;

public interface IComputerOpponentStrategy
{
    Task<string> SelectNextAttack(GameId gameId);
}
