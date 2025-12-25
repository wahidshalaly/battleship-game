using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

public class BoardReadyEvent(GameId gameId, BoardSide boardSide) : DomainEvent<BoardReadyEvent>
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
