using System;
using System.Collections.Generic;

namespace BattleshipChallenge;

public class Cell
{
    private static readonly List<char> _letters = [.. Constants.Alphabet];

    public char Letter { get; }

    public int Digit { get; }

    public string Code { get; }

    public int? ShipId { get; private set; }

    public CellState State { get; private set; } = CellState.Clear;

    public bool HasSameColumn(Cell other) => (Letter == other.Letter);

    public bool HasSameRow(Cell other) => (Digit == other.Digit);

    public Cell(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length is < 2 or > 3)
        {
            throw new ArgumentException(ErrorMessages.InvalidPosition);
        }

        var letter = code[0];
        var digit = int.Parse(code[1..]);

        if (!_letters.Contains(letter))
        {
            throw new ArgumentException(ErrorMessages.InvalidPosition);
        }

        if (digit is <= 0 or > Constants.MaxBoardSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidPosition);
        }

        Letter = letter;
        Digit = digit;
        Code = $"{letter}{digit}";
    }

    public Cell(char letter, int digit)
    {
        if (!_letters.Contains(letter))
        {
            throw new ArgumentException(ErrorMessages.InvalidPosition);
        }

        if (digit is <= 0 or > Constants.MaxBoardSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidPosition);
        }

        Letter = letter;
        Digit = digit;
        Code = $"{letter}{digit}";
    }

    public void Assign(int shipId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(shipId);

        if (State != CellState.Clear)
        {
            throw new ApplicationException(ErrorMessages.InvalidCellToAssign);
        }

        ShipId = shipId;
        State = CellState.Occupied;
    }

    public void Attack()
    {
        if (State == CellState.Hit)
        {
            throw new ApplicationException(ErrorMessages.InvalidCellToHit);
        }
        State = CellState.Hit;
    }

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

public enum CellState
{
    Clear,
    Occupied,
    Hit,
}
