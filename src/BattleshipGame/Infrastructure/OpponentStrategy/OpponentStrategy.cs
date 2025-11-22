using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

public interface IComputerOpponentStrategy
{
    Task<string> SelectNextAttack(GameId gameId);
}

public class RandomAttackStrategy(IGameRepository gameRepository) : IComputerOpponentStrategy
{
    public async Task<string> SelectNextAttack(GameId gameId)
    {
        // Simple: Pick random unattacked cell
        var game =
            await gameRepository.GetByIdAsync(gameId, CancellationToken.None)
            ?? throw new GameNotFoundException(gameId);
        var availableCellCodes = game.GetAvailableCellCodes(BoardSide.Own);
        return availableCellCodes.First();
    }
}

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
