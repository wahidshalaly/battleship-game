using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    [TestFixture]
    public class BattleshipTests
    {
        [Test]
        public void Ctor_CreatesNewShipWithGivenPositions()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });

            var subject = new Battleship(id, positions);

            subject.Id.Should().Be(id);
            subject.Positions.Should().BeEquivalentTo(positions);
            subject.Damages.Should().BeEmpty();
            subject.Sunk.Should().BeFalse();
        }

        [Test]
        public void AttackAt_MarksPositionsOfAttackAsDamaged()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });
            var subject = new Battleship(id, positions);

            subject.AttackAt("bcd");

            var expectedDamages = new List<string>(new[] { "bcd" });
            subject.Damages.Should().BeEquivalentTo(expectedDamages);
            subject.Sunk.Should().BeFalse();
        }

        [Test]
        public void Sunk_WhenAllPositionsAreDamaged_ReturnsTrue()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });
            var subject = new Battleship(id, positions);

            subject.AttackAt("abc");
            subject.AttackAt("bcd");
            subject.AttackAt("cde");

            var expectedDamages = positions;
            subject.Damages.Should().BeEquivalentTo(expectedDamages);
            subject.Sunk.Should().BeTrue();
        }
    }
}