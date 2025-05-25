using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class BoardTests
{
    [Theory]
    [MemberData(nameof(ValidBoardSizes))]
    public void Ctor_InitiatesAllCellsOnBoard(int boardSize, int expectedCellCount)
    {
        var subject = CreateSubject(boardSize);

        subject.Cells.Should().HaveCount(expectedCellCount);
        subject.Ships.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WhenBoardSizeIsLessThanDefaultBoardSize_ThrowsException()
    {
        var random = new Random().Next(1, Board.DefaultSize);
        var invalidBoardSize = Board.DefaultSize - random;

        var subject = () => CreateSubject(invalidBoardSize);

        subject.Should().Throw<ArgumentException>().WithMessage(ErrorMessages.InvalidBoardSize);
    }

    [Theory]
    [MemberData(nameof(ValidShipPositions))]
    public void AddShip_WhenShipPositionIsValid(
        ShipKind shipKind,
        string bowPosition,
        ShipOrientation orientation
    )
    {
        var subject = CreateSubject();

        subject.AddShip(shipKind, bowPosition, orientation);

        subject.Cells.Should().HaveCount(100);
        subject.Ships.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(InvalidShipPositions))]
    public void AddShip_WhenShipPositionsIsInvalid(
        ShipKind shipKind,
        string bowPosition,
        ShipOrientation orientation
    )
    {
        var subject = CreateSubject();

        var act = () => subject.AddShip(shipKind, bowPosition, orientation);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage(ErrorMessages.InvalidShipOnBoardPosition);
        subject.Cells.Should().HaveCount(100);
        subject.Ships.Should().BeEmpty();
    }

    [Fact]
    public void AddShip_WhenShipPositionIntersectWithOccupiedCells_ThrowsException()
    {
        var subject = CreateSubject();
        subject.AddShip(ShipKind.Destroyer, "A1", ShipOrientation.Horizontal);

        var act = () => subject.AddShip(ShipKind.Destroyer, "B1", ShipOrientation.Horizontal);

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidCellToAssign);
    }

    [Fact]
    public void Attack_WhenCellIsValid()
    {
        var subject = CreateSubject();
        subject.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);

        subject.Attack("A1"); // Attack the bow of the ship
        subject.Attack("A2"); // Attack a clear cell

        subject.Cells
            .Where(c => c.State == CellState.Hit)
            .Select(c => c.Code)
            .Should()
            .BeEquivalentTo(["A1", "A2"]);

        subject.Cells.Where(c => c.State == CellState.Occupied).Should().HaveCount(2);
        subject.Cells.Where(c => c.State == CellState.Clear).Should().HaveCount(96);
    }

    [Fact]
    public void IsGameOver_WhenAllShipsAreSunk_ReturnsTrue()
    {
        var subject = CreateSubject();
        subject.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);
        subject.AddShip(ShipKind.Destroyer, "B1", ShipOrientation.Vertical);

        // Attacking the Battleship
        subject.Attack("A1");
        subject.Attack("A4");

        // Sinking the Destroyer
        subject.Attack("B1");
        subject.Attack("B2");

        subject.Ships[0].Sunk.Should().BeFalse(); // Battleship
        subject.Ships[1].Sunk.Should().BeTrue(); // Destroyer

        subject.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void IsGameOver_WhenNotAllShipsAreSunk_ReturnsFalse()
    {
        var subject = CreateSubject();
        subject.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);
        subject.AddShip(ShipKind.Destroyer, "B1", ShipOrientation.Vertical);

        // Sinking the Battleship
        subject.Attack("A1");
        subject.Attack("A2");
        subject.Attack("A3");
        subject.Attack("A4");

        // Sinking the Destroyer
        subject.Attack("B1");
        subject.Attack("B2");

        subject.Ships[0].Sunk.Should().BeTrue(); // Battleship
        subject.Ships[1].Sunk.Should().BeTrue(); // Destroyer

        subject.IsGameOver.Should().BeTrue();
    }

    public static TheoryData<int, int> ValidBoardSizes =>
        new()
        {
            { 10, 100 },
            { 15, 225 },
            { 20, 400 },
            { 26, 676 },
        };

    public static TheoryData<ShipKind, string, ShipOrientation> ValidShipPositions =>
        new()
        {
            { ShipKind.Destroyer, "A1", ShipOrientation.Vertical },
            { ShipKind.Submarine, "B2", ShipOrientation.Horizontal },
            { ShipKind.Cruiser, "C3", ShipOrientation.Vertical },
            { ShipKind.Battleship, "D4", ShipOrientation.Horizontal },
        };

    public static TheoryData<ShipKind, string, ShipOrientation> InvalidShipPositions =>
        new()
        {
            { ShipKind.Destroyer, "Z1", ShipOrientation.Vertical },
            { ShipKind.Submarine, "B20", ShipOrientation.Horizontal },
            { ShipKind.Cruiser, "C0", ShipOrientation.Vertical },
            { ShipKind.Battleship, "D-1", ShipOrientation.Horizontal },
        };

    private static Board CreateSubject(int size = 10) => new(size);
}
