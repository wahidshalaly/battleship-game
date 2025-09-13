using BattleshipGame.Domain.Common;
using BattleshipGame.SharedKernel;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

internal class Cell : ValueObject
{
    private static readonly HashSet<char> _letters = [.. ColumnHeaders];

    public char Letter { get; }

    public int Digit { get; }

    public string Code { get; }

    public ShipId? ShipId { get; private set; }

    public CellState State { get; private set; } = CellState.Clear;

    public Cell(char letter, int digit)
    {
        if (!_letters.Contains(letter) || digit is <= 0 or > MaximumBoardSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        Letter = letter;
        Digit = digit;
        Code = $"{letter}{digit}";
    }

    public void Assign(ShipId shipId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(shipId.Value, Guid.Empty);

        if (State != CellState.Clear)
            throw new InvalidOperationException(ErrorMessages.InvalidCellToAssign);

        ShipId = shipId;
        State = CellState.Occupied;
    }

    public void Attack()
    {
        State = State switch
        {
            CellState.Clear => CellState.Missed,
            CellState.Occupied => CellState.Hit,
            CellState.Hit or CellState.Missed => throw new InvalidOperationException(ErrorMessages.InvalidCellToAttack),
            _ => throw new ArgumentOutOfRangeException(nameof(State), State, null),
        };
    }

    public static (char Letter, int Digit) FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length is < 2 or > 3)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        var letter = code[0];
        _ = int.TryParse(code[1..], out var digit);
        if (!_letters.Contains(letter) || digit is <= 0 or > MaximumBoardSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        return (letter, digit);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override bool Equals(object? obj) => obj is Cell c && Code == c.Code;

    public override int GetHashCode() => HashCode.Combine(Code);
}
