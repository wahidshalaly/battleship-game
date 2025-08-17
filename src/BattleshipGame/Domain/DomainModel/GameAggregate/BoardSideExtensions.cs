using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

public static class BoardSideExtensions
{
    public static BoardSide OppositeSide(this BoardSide side)
    {
        return side switch
        {
            BoardSide.Own => BoardSide.Opp,
            BoardSide.Opp => BoardSide.Own,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, ErrorMessages.InvalidBoardSide)
        };
    }
}