using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class BattleshipTests
{
    private const int ShipId = 3;

    [Fact]
    public void Ctor_CreatesNewShipWithGivenCells()
    {
        List<Cell> cells = [(Cell)"A1", (Cell)"A2", (Cell)"A3"];

        var subject = new Battleship(ShipId, cells);

        subject.Id.Should().Be(ShipId);
        subject.Cells.Should().BeEquivalentTo(cells);
        subject.Cells.Select(c => c.IsHit).Should().BeEmpty();
        subject.Sunk.Should().BeFalse();
    }

    [Fact]
    public void AttackAt_MarksCellsOfAttackAsDamaged()
    {
        List<Cell> cells = [(Cell)"A1", (Cell)"A2", (Cell)"A3"];
        var subject = new Battleship(ShipId, cells);

        subject.AttackAt((Cell)"A2");

        List<Cell> expectedDamages = [(Cell)"A2"];
        subject.Cells.Select(c => c.IsHit).ToArray().Should().BeEquivalentTo(expectedDamages);
        subject.Sunk.Should().BeFalse();
    }

    [Fact]
    public void Sunk_WhenAllCellsAreDamaged_ReturnsTrue()
    {
        List<Cell> cells = [(Cell)"A1", (Cell)"A2", (Cell)"A3"];
        var subject = new Battleship(ShipId, cells);

        subject.AttackAt((Cell)"A1");
        subject.AttackAt((Cell)"A2");
        subject.AttackAt((Cell)"A3");

        subject.Cells.Select(c => c.IsHit).Should().BeEquivalentTo(cells);
        subject.Sunk.Should().BeTrue();
    }
}