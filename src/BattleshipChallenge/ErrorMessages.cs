namespace BattleshipChallenge;

public static class ErrorMessages
{
    // Cell Error Messages
    public const string InvalidCellCode =
        "Invalid cell code. A cell code consists of a letter (A to Z) and a digit (1 to 26).\n" +
        "But it shouldn't exceed board size, e.g. J10 is maximum value in board of size 10.";

    public const string InvalidCellToAssign =
        "Cell is already assigned and cannot be assigned again.";

    public const string InvalidCellToHit =
        "Cell is already hit and cannot be attacked againt.";

    // Board Error Messages
    public const string InvalidCellOutOfRange =
        "Cell must be defined on the board.";

    public const string InvalidBoardSize =
        "Invalid board size. It must be a positive number and it doesn't exceed the maximum board size of 26 squares.";

    public const string InvalidShipLocation =
        "Ship must be celled either vertically or horizontally.";

    // Ship Error Messages
    // Game Error Messages
    public const string InvalidShipSize =
        "Ship size must be between 1 and the maximum board size of 26 squares.";
}