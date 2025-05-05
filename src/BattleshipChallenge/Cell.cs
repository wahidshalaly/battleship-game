using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge;

public class Cell
{
    private static readonly List<char> _letters = Constants.Alphabet.ToList();

    public char Letter { get; }
    public int Digit { get; }

    private Cell(char letter, int digit)
    {
        if (!_letters.Contains(letter))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellCode, nameof(letter));
        }

        if (digit is <= 0 or > Constants.MaxBoardSize)
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellCode, nameof(digit));
        }

        Letter = letter;
        Digit = digit;
    }

    private static Cell FromString(string cell)
    {
        try
        {
            var letter = cell[0];
            var digit = int.Parse(cell[1..]);
            return new Cell(letter, digit);
        }
        catch
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellCode, nameof(cell));
        }
    }

    public override string ToString() => $"{Letter}{Digit}";

    public bool HasSameColumn(Cell other) => (Letter == other.Letter);

    public bool HasSameRow(Cell other) => (Digit == other.Digit);

    public static explicit operator Cell(string cell) => FromString(cell);

    public static implicit operator string(Cell cell) => cell.ToString();

    // Overload ==
    public static bool operator ==(Cell c1, Cell c2)
    {
        if (ReferenceEquals(c1, c2))
            return true;

        if (c1 is null || c2 is null)
            return false;

        return c1.HasSameColumn(c2) && c1.HasSameRow(c2);
    }

    // Overload !=
    public static bool operator !=(Cell c1, Cell c2)
    {
        return !(c1 == c2);
    }

    // Override Equals
    public override bool Equals(object obj)
    {
        return obj is Cell p && this == p;
    }

    // Override GetHashCode
    public override int GetHashCode()
    {
        return HashCode.Combine(Letter, Digit);
    }
}