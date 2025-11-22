using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

public class BoardSideReadyEvent(GameId gameId, BoardSide boardSide)
    : DomainEvent<BoardSideReadyEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;

    /// <summary>
    /// Gets the board side that is ready.
    /// </summary>
    public BoardSide BoardSide { get; } = boardSide;
}
