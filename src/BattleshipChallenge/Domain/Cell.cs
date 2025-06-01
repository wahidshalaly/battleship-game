using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipChallenge.Common;
using BattleshipChallenge.Domain.Base;

namespace BattleshipChallenge.Domain;

internal class Cell : ValueObject
{
    private static readonly HashSet<char> _letters = [.. Constants.Alphabet];

    public char Letter { get; }

    public int Digit { get; }

    public string Code { get; }

    public int? ShipId { get; private set; }

    public CellState State { get; private set; } = CellState.Clear;

    public Cell(char letter, int digit)
    {
        if (!_letters.Contains(letter))
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        if (digit is <= 0 or > Board.MaximumSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
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
            throw new ApplicationException(ErrorMessages.InvalidCellToAttack);
        }
        State = CellState.Hit;
    }

    public static (char Letter, int Digit) FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length is < 2 or > 3)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        var letter = code[0];
        _ = int.TryParse(code[1..], out var digit);
        if (!_letters.Contains(letter) || digit is <= 0 or > Board.MaximumSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        return (letter, digit);
    }

    // Override Equals
    public override bool Equals(object obj) => obj is Cell c && Code == c.Code;

    // Override GetHashCode
    public override int GetHashCode() => HashCode.Combine(Code);
}
