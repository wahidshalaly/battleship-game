using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge;

public interface ICellLocator
{
    /// <summary>
    /// This returns cells between two cells inclusively.
    /// </summary>
    /// <param name="bow">A valid location on the board</param>
    /// <param name="stern">A valid location on the board</param>
    /// <returns>All cells between given two cells inclusively, if they share same column or row.</returns>
    IEnumerable<Cell> FindCells(Cell bow, Cell stern);

    /// <summary>
    /// Returns all possible cells on a board of a given size
    /// </summary>
    /// <param name="size">A given size for a board (any positive number)</param>
    /// <returns></returns>
    IEnumerable<Cell> GetAllCellsOnBoardOf(int size);
}

internal class CellLocator : ICellLocator
{
    private static readonly List<char> _letters = Constants.Alphabet.ToList();

    public IEnumerable<Cell> FindCells(Cell bow, Cell stern)
    {
        ArgumentNullException.ThrowIfNull(bow);
        ArgumentNullException.ThrowIfNull(stern);

        if (!bow.HasSameColumn(stern) && !bow.HasSameRow(stern))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidShipLocation);
        }

        if (bow == stern)
        {
            yield return bow;
            yield break;
        }

        if (bow.Letter == stern.Letter)
        {
            var minDigit = Math.Min(bow.Digit, stern.Digit);
            var maxDigit = Math.Max(bow.Digit, stern.Digit);

            for (var digit = minDigit; digit <= maxDigit; digit++)
            {
                yield return (Cell)$"{bow.Letter}{digit}";
            }
        }
        else
        {
            var minIndex = Math.Min(_letters.IndexOf(bow.Letter), _letters.IndexOf(stern.Letter));
            var maxIndex = Math.Max(_letters.IndexOf(bow.Letter), _letters.IndexOf(stern.Letter));

            for (var idx = minIndex; idx <= maxIndex; idx++)
            {
                yield return (Cell)$"{_letters[idx]}{bow.Digit}";
            }
        }
    }

    public IEnumerable<Cell> GetAllCellsOnBoardOf(int size)
    {
        if (size is <= 0 or > Constants.MaxBoardSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, Constants.ErrorMessages.InvalidBoardSize);
        }

        var letters = _letters.GetRange(0, size);
        var digits = Enumerable.Range(1, size).ToArray();

        foreach (var letter in letters)
        {
            foreach (var digit in digits)
            {
                yield return (Cell)$"{letter}{digit}";
            }
        }
    }
}