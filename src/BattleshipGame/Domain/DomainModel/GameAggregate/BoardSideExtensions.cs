using BattleshipGame.Domain.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

public static class BoardSideExtensions
{
    public static BoardSide OppositeSide(this BoardSide side)
    {
        return side switch
        {
            BoardSide.Player => BoardSide.Opponent,
            BoardSide.Opponent => BoardSide.Player,
            _ => throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                ErrorMessages.InvalidBoardSide
            ),
        };
    }
}
