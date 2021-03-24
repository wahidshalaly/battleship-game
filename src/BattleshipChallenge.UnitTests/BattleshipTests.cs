using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    public class BattleshipTests
    {
        [Test]
        public void Ctor_CreatesNewShipWithGivenPositions()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });
            var ship = new Battleship(id, positions);

            ship.Id.Should().Be(id);
            ship.Positions.Should().BeEquivalentTo(positions);
            ship.Damages.Should().BeEmpty();
            ship.Sunk.Should().BeFalse();
        }

        [Test]
        public void AttackAt_MarksPositionsOfAttackAsDamaged()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });
            var ship = new Battleship(id, positions);

            ship.AttackAt("bcd");

            var expectedDamages = new List<string>(new[] { "bcd" });
            ship.Damages.Should().BeEquivalentTo(expectedDamages);

            ship.Sunk.Should().BeFalse();
        }

        [Test]
        public void Sunk_WhenAllPositionsAreDamaged_ReturnsTrue()
        {
            var id = 3;
            var positions = new List<string>(new[] { "abc", "bcd", "cde" });
            var ship = new Battleship(id, positions);

            ship.AttackAt("abc");
            ship.AttackAt("bcd");
            ship.AttackAt("cde");

            var expectedDamages = positions;
            ship.Damages.Should().BeEquivalentTo(expectedDamages);

            ship.Sunk.Should().BeTrue();
        }
    }
}