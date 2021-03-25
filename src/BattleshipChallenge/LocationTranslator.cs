using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge
{
    public interface ILocationTranslator
    {
        /// <summary>
        /// Returns positions between two positions inclusively.
        /// </summary>
        /// <param name="bow">Any valid location on the board</param>
        /// <param name="stern">Any valid location on the board</param>
        /// <returns>All positions between given two positions inclusively</returns>
        IEnumerable<string> FindPositions(string bow, string stern);

        /// <summary>
        /// Returns all possible positions on a board of a given size
        /// </summary>
        /// <param name="size">A given size for a board (any positive number)</param>
        /// <returns></returns>
        IEnumerable<string> GetAllPositionsOnBoardOf(int size);
    }

    internal class LocationTranslator : ILocationTranslator
    {
        private static readonly List<char> _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();

        public IEnumerable<string> FindPositions(string bow, string stern)
        {
            var bowLetter = bow[0];
            var bowDigit = int.Parse(bow.Substring(1));
            var sternLetter = stern[0];
            var sternDigit = int.Parse(stern.Substring(1));

            if (bowLetter == sternLetter && bowDigit == sternDigit)
            {
                yield return bow;
                yield break;
            }

            if (bowLetter == sternLetter)
            {
                var x = Math.Min(bowDigit, sternDigit);
                var y = Math.Max(bowDigit, sternDigit);
                for (var digit = x; digit <= y; digit++)
                {
                    yield return $"{bowLetter}{digit}";
                }
                yield break;
            }

            if (bowDigit == sternDigit)
            {
                var a = _letters.IndexOf(bowLetter);
                var b = _letters.IndexOf(sternLetter);
                var x = Math.Min(a, b);
                var y = Math.Max(a, b);
                for (var idx = x; idx <= y; idx++)
                {
                    yield return $"{_letters[idx]}{bowDigit}";
                }
            }
        }

        public IEnumerable<string> GetAllPositionsOnBoardOf(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            var letters = _letters.GetRange(0, size);
            var digits = Enumerable.Range(1, 10).ToArray();

            foreach (var letter in letters)
            {
                foreach (var digigt in digits)
                {
                    yield return $"{letter}{digigt}";
                }
            }
        }
    }
}