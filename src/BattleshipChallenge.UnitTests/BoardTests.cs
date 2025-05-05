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
        subject.Cells.ContainsKey("A1").Should().BeTrue();
        subject.Cells.ContainsKey("J10").Should().BeTrue();
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
        var expectedCells = _locator.FindCells(bow, stern).ToArray();
        var subject = CreateSubject();

        subject.AddShip(bow, stern);

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
            subject.AddShip(bow, stern);

            var expectedCells = _locator.FindCells(bow, stern).ToArray();

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
        subject.Cells.Where(p => p.Value == 1).Should().HaveCount(4);
        subject.Cells.Where(p => p.Value == 2).Should().HaveCount(2);
        subject.Cells.Values.Where(v => v != null).Should().HaveCount(9);
    }

    [Theory]
    [InlineData("A1", "J10")]
    [InlineData("D4", "C7")]
    public void AddShip_WhenLocationIsInvalid_ThrowsException(string bow, string stern)
    {
        var subject = CreateSubject();

        subject
            .Invoking(s => s.AddShip(bow, stern))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(Constants.ErrorMessages.InvalidShipLocation);
    }

    [Theory]
    [InlineData("D40", "C40")]
    public void AddShip_WhenLocationOutsideOfBoard_ThrowsException(string bow, string stern)
    {
        var subject = CreateSubject();

        subject
            .Invoking(s => s.AddShip(bow, stern))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(Constants.ErrorMessages.InvalidCellOutOfRange);
    }

    [Fact]
    public void Attack_WhenCellIsInvalid()
    {
        var subject = CreateSubject();

        subject
            .Invoking(s => s.Attack("C40"))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(Constants.ErrorMessages.InvalidCellOutOfRange);
    }

    [Fact]
    public void Attack_WhenCellIsValidButNotOccupied_ReturnFalse()
    {
        var subject = CreateSubject();

        var result = subject.Attack("C4");

        result.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenCellIsValidAndOccupied_ReturnTrue()
    {
        var subject = CreateSubject();
        subject.AddShip("C3", "C5");

        var result = subject.Attack("C4");

        result.Should().BeTrue();
        subject.Attacks.Should().HaveCount(1);
        subject.Ships[0].Damages.Should().HaveCount(1);
        subject.Ships[0].Sunk.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenAllShipAttack_ShouldSink()
    {
        var subject = CreateSubject();
        subject.AddShip("C3", "C5");

        _ = subject.Attack("C3");
        _ = subject.Attack("C4");
        _ = subject.Attack("C5");

        subject.Attacks.Should().HaveCount(3);
        subject.Ships[0].Damages.Should().HaveCount(3);
        subject.Ships[0].Sunk.Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSink_ReturnsTrue()
    {
        var subject = CreateSubject();
        subject.AddShip("C3", "C5");
        subject.AddShip("A7", "C7");
        // Attack ship 1 - sunk
        _ = subject.Attack("C3");
        _ = subject.Attack("C4");
        _ = subject.Attack("C5");
        // Attack ship 2 - sunk
        _ = subject.Attack("A7");
        _ = subject.Attack("B7");
        _ = subject.Attack("C7");

        subject.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAnyShipStillAfloat_ReturnsFalse()
    {
        var subject = CreateSubject();
        subject.AddShip("C3", "C5");
        subject.AddShip("A7", "C7");
        // Attack ship 1 - not sunk
        _ = subject.Attack("C3");
        _ = subject.Attack("C5");
        // Attack ship 2 - sunk
        _ = subject.Attack("A7");
        _ = subject.Attack("B7");
        _ = subject.Attack("C7");

        subject.IsGameOver.Should().BeFalse();
    }

    private Board CreateSubject()
    {
        return new Board(_locator);
    }
}