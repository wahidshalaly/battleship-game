using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class BattleshipTests
{
    private const int ShipId = 3;

    [Fact]
    public void Ctor_CreatesNewShipWithGivenCells()
    {
        var cells = new List<string>(["abc", "bcd", "cde"]);

        var subject = new Battleship(ShipId, cells);

        subject.Id.Should().Be(ShipId);
        subject.Cells.Should().BeEquivalentTo(cells);
        subject.Damages.Should().BeEmpty();
        subject.Sunk.Should().BeFalse();
    }

    [Fact]
    public void AttackAt_MarksCellsOfAttackAsDamaged()
    {
        var cells = new List<string>(["abc", "bcd", "cde"]);
        var subject = new Battleship(ShipId, cells);

        subject.AttackAt("bcd");

        var expectedDamages = new List<string>(["bcd"]);
        subject.Damages.Should().BeEquivalentTo(expectedDamages);
        subject.Sunk.Should().BeFalse();
    }

    [Fact]
    public void Sunk_WhenAllCellsAreDamaged_ReturnsTrue()
    {
        var cells = new List<string>(["abc", "bcd", "cde"]);
        var subject = new Battleship(ShipId, cells);

        subject.AttackAt("abc");
        subject.AttackAt("bcd");
        subject.AttackAt("cde");

        subject.Damages.Should().BeEquivalentTo(cells);
        subject.Sunk.Should().BeTrue();
    }
}