using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

public class RandomAttackStrategy(IGameRepository gameRepository) : IComputerOpponentStrategy
{
    public async Task<string> SelectNextAttack(GameId gameId)
    {
        // Simple: Pick random unattacked cell
        var game =
            await gameRepository.GetByIdAsync(gameId, CancellationToken.None)
            ?? throw new GameNotFoundException(gameId);
        var availableCellCodes = game.GetAvailableCellCodes(BoardSide.Player);
        return availableCellCodes.First();
    }
}