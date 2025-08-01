using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Domain.Application;

public interface IGameService
{
    public GameId StartGame(Guid player1, Guid player2);
}
