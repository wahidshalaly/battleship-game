using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    [TestFixture]
    public class BoardTests
    {
        [Test]
        public void Ctor_InitiatesAllPositionsOnBoard()
        {
            var translator = new LocationTranslator();
            var board = new Board(translator);
            board.Positions.Distinct()
                .Should().HaveCount(Board.BoardSize * Board.BoardSize);

            board.Ships.Should().BeEmpty();
            board.Attacks.Should().BeEmpty();
        }
    }
}