using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Contracts.OpponentStrategy;

public interface IComputerOpponentStrategy
{
    /// <summary>
    /// Selects the next attack cell for the computer opponent.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <returns>The cell code for the next attack (e.g., "A1").</returns>
    Task<string> SelectNextAttack(GameId gameId);
}
