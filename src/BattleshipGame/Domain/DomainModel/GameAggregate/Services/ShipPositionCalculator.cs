using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Services;

/// <inheritdoc />
internal class ShipPositionCalculator : IShipPositionCalculator
{
    private static readonly List<char> _letters = [.. ColumnHeaders];

    /// <inheritdoc />
    public List<string> CalculatePosition(int boardSize, ShipKind kind, ShipOrientation orientation, string bowCode)
    {
        // Implementation of position calculation logic based on ship kind, orientation, and bow code.
        // This should return a list of cell codes that the ship occupies.
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void ValidatePosition(int boardSize, ShipKind kind, ShipOrientation orientation, string bowCode)
    {
        ValidateShipKind(kind);
        ValidateOrientation(orientation);
        ValidateCellCode(bowCode, boardSize);

        var sternCode = CalculateSternCode(kind, orientation, bowCode);
        ValidateCellCode(sternCode, boardSize);
    }

    /// <inheritdoc />
    public void ValidateOrientation(ShipOrientation orientation)
    {
        if (orientation == ShipOrientation.None)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOrientation);
        }
    }

    /// <inheritdoc />
    public void ValidateShipKind(ShipKind kind)
    {
        if (kind == ShipKind.None)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipKind);
        }
    }

    private static string CalculateSternCode(ShipKind kind, ShipOrientation orientation, string bowCode)
    {
        var (letter, digit) = Cell.FromCode(bowCode);
        var shipSize = kind.ToSize();

        // Calculate the stern cell based on ship size and orientation
        var sternCode =
            orientation == ShipOrientation.Vertical
                ? $"{letter}{digit + shipSize - 1}"
                : $"{_letters[_letters.IndexOf(letter) + shipSize - 1]}{digit}";

        return sternCode;
    }

    private static void ValidateCellCode(string cellCode, int boardSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellCode);

        if (string.IsNullOrWhiteSpace(cellCode) || cellCode.Length < 2)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);
        }

        var letter = cellCode[0];
        var digit = cellCode[1..];
        var isValid = _letters.Contains(letter) && int.TryParse(digit, out var num) && num > 0 && num <= boardSize;
        if (!isValid)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);
        }
    }
}
