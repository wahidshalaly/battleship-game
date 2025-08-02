using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

internal class Cell : ValueObject
{
    private static readonly HashSet<char> _letters = [.. Constants.ColumnHeaders];

    public char Letter { get; }

    public int Digit { get; }

    public string Code { get; }

    public ShipId? ShipId { get; private set; }

    public CellState State { get; private set; } = CellState.Clear;

    public Cell(char letter, int digit)
    {
        if (!_letters.Contains(letter) || digit is <= 0 or > Board.MaximumSize)
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
            throw new ApplicationException(ErrorMessages.InvalidCellToAssign);

        ShipId = shipId;
        State = CellState.Occupied;
    }

    public void Attack()
    {
        if (State is CellState.Hit or CellState.Miss)
        {
            throw new ApplicationException(ErrorMessages.InvalidCellToAttack);
        }
        //State = (State is CellState.Clear or CellState.Occupied) CellState.Hit;
        switch (State)
        {
            case CellState.Clear:
                State = CellState.Miss;
                break;
            case CellState.Occupied:
                State = CellState.Hit;
                break;
            case CellState.Hit:
            case CellState.Miss:
                throw new ApplicationException(ErrorMessages.InvalidCellToAttack);
            default:
                throw new ArgumentOutOfRangeException();
        }
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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override bool Equals(object? obj) => obj is Cell c && Code == c.Code;

    public override int GetHashCode() => HashCode.Combine(Code);
}
