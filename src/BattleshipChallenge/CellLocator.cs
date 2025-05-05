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
    IEnumerable<string> FindCells(string bow, string stern);

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

    public IEnumerable<string> FindCells(string bow, string stern)
    {
        if (string.IsNullOrWhiteSpace(bow) || string.IsNullOrWhiteSpace(stern))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellCode);
        }

        var bowCell = (Cell)bow;
        var sternCell = (Cell)stern;

        if (!bowCell.HasSameColumn(sternCell) && !bowCell.HasSameRow(sternCell))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidShipLocation);
        }

        if (bowCell == sternCell)
        {
            yield return bow;
            yield break;
        }

        if (bowCell.Letter == sternCell.Letter)
        {
            var minDigit = Math.Min(bowCell.Digit, sternCell.Digit);
            var maxDigit = Math.Max(bowCell.Digit, sternCell.Digit);

            for (var digit = minDigit; digit <= maxDigit; digit++)
            {
                yield return $"{bowCell.Letter}{digit}";
            }
        }
        else
        {
            var minIndex = Math.Min(_letters.IndexOf(bowCell.Letter), _letters.IndexOf(sternCell.Letter));
            var maxIndex = Math.Max(_letters.IndexOf(bowCell.Letter), _letters.IndexOf(sternCell.Letter));

            for (var idx = minIndex; idx <= maxIndex; idx++)
            {
                yield return $"{_letters[idx]}{bowCell.Digit}";
            }
        }
    }

    public IEnumerable<Cell> GetAllCellsOnBoardOf(int size)
    {
        if (size <= 0 || size > Constants.MaxBoardSize)
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