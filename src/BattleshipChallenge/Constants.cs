namespace BattleshipChallenge;

public static class Constants
{
    public const int MaxBoardSize = 26;
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static class ErrorMessages
    {
        public const string InvalidCellCode =
            "Invalid cell format. Cell code consists of a letter (A to Z) and a digit (1 to 26).\n" +
            "But it shouldn't exceed board size, e.g. J10 is maximum value in board of size 10.";

        public const string InvalidCellOutOfRange =
            "Selected cell must be defined on the board";

        public const string InvalidShipLocation =
            "Ship must be celled either vertically or horizontally";

        public const string InvalidCellToHit =
            "Selected cell must not have been attacked before";

        public const string InvalidBoardSize =
            "Invalid board size. It must be a positive number and it doesn't exceed the maximum board size of 26 squares.";

        public const string InvalidShipSize =
            "Ship size must be between 1 and the maximum board size of 26 squares";
    }
}