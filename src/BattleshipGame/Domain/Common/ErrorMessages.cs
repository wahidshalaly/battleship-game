namespace BattleshipGame.Domain.Common;

internal static class ErrorMessages
{
    // Cell Error Messages
    public const string InvalidCellCode =
        "Invalid cell code. A cell code consists of a letter (A to Z) and a digit (1 to 26).\n"
        + "But it shouldn't exceed board size, e.g. J10 is maximum value in board of size 10.";

    public const string InvalidCellToAssign =
        "Cell is already assigned and cannot be assigned again.";

    public const string InvalidCellToAttack = "Cell is already hit and cannot be attacked againt.";

    // Board Error Messages
    public const string InvalidShipOnBoardPosition = "Invalid ship position on board.";

    public const string InvalidBoardSize =
        "Invalid board size. It must be a positive number and it doesn't exceed the maximum board size of 26 squares.";

    public const string InvalidShipKind =
        "Invalid ship kind. `None` is not a valid value.";

    public const string InvalidShipKindAlreadyExists =
        "Invalid ship kind. You cannot add a ship kind more than once.";

    public const string InvalidShipAddition =
        "Invalid ship addition. You cannot add ships more than the maximum of board allowance.";

    // Ship Error Messages
    public const string InvalidShipPosition_Count =
        "Invalid cell count. Number of cells mush match exactly the size of the ship.";

    public const string InvalidShipPosition_Alignment =
        "Invalid alignment. All cells must have the same row or same column.";

    public const string InvalidShipAttack = "Cell does not belong to ship position.";

    public const string InvalidBoardSide =
        "Invalid board side. Allowed values are either `Own` or `Opp`.";

    public const string InvalidShipOrientation =
        "Invalid ship orientation. Allowed values are either `Horizontal` or `Vertical`.";
}
