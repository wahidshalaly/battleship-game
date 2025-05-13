using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class BoardTests
{
    private readonly CellLocator _locator = new();

    [Fact]
    public void Ctor_InitiatesAllCellsOnBoard()
    {
        var subject = CreateSubject();

        subject.Cells.Should().HaveCount(100);
        subject.Cells.ContainsKey((Cell)"A1").Should().BeTrue();
        subject.Cells.ContainsKey((Cell)"J10").Should().BeTrue();
        subject.Ships.Should().BeEmpty();
        subject.Attacks.Should().BeEmpty();
    }

    [Theory]
    [InlineData("D4", "D4")]
    [InlineData("D4", "D7")]
    [InlineData("D7", "D4")]
    [InlineData("A4", "D4")]
    public void AddShip_WhenLocationIsValid_AddsShipToBoard(string bow, string stern)
    {
        var expectedCells = _locator.FindCellsBetween((Cell)bow, (Cell)stern).ToArray();
        var subject = CreateSubject();

        subject.AddShip((Cell)bow, (Cell)stern);

        // validate that ship has been successfully created
        subject.Ships.Should().HaveCount(1);
        subject.Ships[0].Id.Should().Be(0);
        subject.Ships[0].Cells.Should().BeEquivalentTo(expectedCells);

        // validate that board has marked the ship location by ship-id (0)
        subject
            .Cells.Count(p => expectedCells.Contains(p.Key) && p.Value == 0)
            .Should()
            .Be(expectedCells.Length);
    }

    [Fact]
    public void AddShip_WhenAddingMultipleShips()
    {
        var shipId = 0;
        var ships = new[]
        {
            new Tuple<string, string>("C4", "C6"),
            new Tuple<string, string>("J10", "G10"),
            new Tuple<string, string>("F8", "G8"),
        };
        var subject = CreateSubject();

        foreach (var (bow, stern) in ships)
        {
            subject.AddShip((Cell)bow, (Cell)stern);

            var expectedCells = _locator.FindCellsBetween((Cell)bow, (Cell)stern).ToArray();

            // validate that ship has been successfully created
            subject.Ships[shipId].Id.Should().Be(shipId);
            subject.Ships[shipId].Cells.Should().BeEquivalentTo(expectedCells);

            // validate that board has marked the ship location
            subject
                .Cells.Count(p => expectedCells.Contains(p.Key) && p.Value == shipId)
                .Should()
                .Be(expectedCells.Length);

            shipId++;
        }

        // validate that ship has been successfully created
        subject.Ships.Should().HaveCount(3);
        subject.Cells.Where(p => p.Value == 0).Should().HaveCount(3);
    }

    [Theory]
    [InlineData("A1", "J10")]
    [InlineData("D4", "C7")]
    public void AddShip_WhenLocationIsInvalid_ThrowsException(string bow, string stern)
    {
        var subject = CreateSubject();

        subject
            .Invoking(s => s.AddShip((Cell)bow, (Cell)stern))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(Constants.ErrorMessages.InvalidShipLocation);
    }

    [Fact]
    public void Attack_WhenCellIsValidButNotOccupied_ReturnFalse()
    {
        var subject = CreateSubject();

        var result = subject.Attack((Cell)"C4");

        result.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenCellIsValidAndOccupied_ReturnTrue()
    {
        var subject = CreateSubject();
        subject.AddShip((Cell)"C3", (Cell)"C5");

        var result = subject.Attack((Cell)"C4");

        result.Should().BeTrue();
        subject.Attacks.Should().HaveCount(1);
        subject.Ships[0].Cells.Select(c => c.IsHit).Should().HaveCount(1);
        subject.Ships[0].Sunk.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenAllShipAttack_ShouldSink()
    {
        var subject = CreateSubject();
        subject.AddShip((Cell)"C3", (Cell)"C5");

        _ = subject.Attack((Cell)"C3");
        _ = subject.Attack((Cell)"C4");
        _ = subject.Attack((Cell)"C5");

        subject.Attacks.Should().HaveCount(3);
        subject.Ships[0].Cells.Select(c => c.IsHit).Should().HaveCount(3);
        subject.Ships[0].Sunk.Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSink_ReturnsTrue()
    {
        var subject = CreateSubject();
        subject.AddShip((Cell)"C3", (Cell)"C5");
        subject.AddShip((Cell)"A7", (Cell)"C7");
        // Attack ship 1 - sunk
        _ = subject.Attack((Cell)"C3");
        _ = subject.Attack((Cell)"C4");
        _ = subject.Attack((Cell)"C5");
        // Attack ship 2 - sunk
        _ = subject.Attack((Cell)"A7");
        _ = subject.Attack((Cell)"B7");
        _ = subject.Attack((Cell)"C7");

        subject.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAnyShipStillAfloat_ReturnsFalse()
    {
        var subject = CreateSubject();
        subject.AddShip((Cell)"C3", (Cell)"C5");
        subject.AddShip((Cell)"A7", (Cell)"C7");
        // Attack ship 1 - not sunk
        _ = subject.Attack((Cell)"C3");
        _ = subject.Attack((Cell)"C5");
        // Attack ship 2 - sunk
        _ = subject.Attack((Cell)"A7");
        _ = subject.Attack((Cell)"B7");
        _ = subject.Attack((Cell)"C7");

        subject.IsGameOver.Should().BeFalse();
    }

    private Board CreateSubject()
    {
        return new Board(_locator);
    }
}