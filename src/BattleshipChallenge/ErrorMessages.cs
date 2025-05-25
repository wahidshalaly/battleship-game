namespace BattleshipChallenge;

public static class ErrorMessages
{
    // Cell Error Messages
    public const string InvalidCellCode =
        "Invalid cell code. A cell code consists of a letter (A to Z) and a digit (1 to 26).\n"
        + "But it shouldn't exceed board size, e.g. J10 is maximum value in board of size 10.";

    public const string InvalidCellToAssign =
        "Cell is already assigned and cannot be assigned again.";

    public const string InvalidCellToHit = "Cell is already hit and cannot be attacked againt.";

    // Board Error Messages
    public const string InvalidShipOnBoardPosition = "Invalid ship position on board.";

    public const string InvalidBoardSize =
        "Invalid board size. It must be a positive number and it doesn't exceed the maximum board size of 26 squares.";

    // Ship Error Messages
    public const string InvalidShipPosition_Count =
        "Invalid cell count. Number of cells mush match exactly the size of the ship.";

    public const string InvalidShipPosition_Alignment =
        "Invalid alignment. All cells must have the same row or same column.";
}
