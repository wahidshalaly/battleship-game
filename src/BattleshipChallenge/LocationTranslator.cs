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
            var bowDigit = int.Parse(bow[1].ToString());
            var sternLetter = stern[0];
            var sternDigit = int.Parse(stern[1].ToString());

            if (bowLetter == sternLetter && bowDigit == sternDigit)
            {
                yield return bow;
                yield break;
            }

            if (bowLetter == sternLetter)
            {
                var x = Math.Min(bowDigit, sternDigit);
                var y = Math.Max(bowDigit, sternDigit);
                for (var horizontal = x; horizontal <= y; horizontal++)
                {
                    yield return $"{bowLetter}{horizontal}";
                }
                yield break;
            }

            if (bowDigit == sternDigit)
            {
                var a = _letters.IndexOf(bowLetter);
                var b = _letters.IndexOf(sternLetter);
                var x = Math.Min(a, b);
                var y = Math.Max(a, b);
                for (var vertical = x; vertical <= y; vertical++)
                {
                    yield return $"{_letters[vertical]}{bowDigit}";
                }
            }
        }

        public IEnumerable<string> GetAllPositionsOnBoardOf(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            var verticals = _letters.GetRange(0, size);
            var horizontals = Enumerable.Range(1, 10).ToArray();

            foreach (var v in verticals)
            {
                foreach (var h in horizontals)
                {
                    yield return $"{v}{h}";
                }
            }
        }
    }
}