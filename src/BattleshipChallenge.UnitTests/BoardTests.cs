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

            var subject = new Board(translator);

            subject.Positions.Distinct().Should().HaveCount(Board.BoardSize * Board.BoardSize);
            subject.Positions.ContainsKey("A1").Should().BeTrue();
            subject.Positions.ContainsKey("J10").Should().BeTrue();
            subject.Ships.Should().BeEmpty();
            subject.Attacks.Should().BeEmpty();
        }
    }
}