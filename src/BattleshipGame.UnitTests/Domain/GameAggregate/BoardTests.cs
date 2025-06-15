using System;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.Domain;
using BattleshipGame.Domain.GameAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.GameAggregate;

public class BoardTests
{
    [Theory]
    [MemberData(nameof(ValidBoardSizes))]
    public void Ctor_InitiatesAllCellsOnBoard(int boardSize, int expectedCellCount)
    {
        var board = new Board(boardSize);

        board.Cells.Should().HaveCount(expectedCellCount);
        board.Ships.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WhenBoardSizeIsLessThanDefaultBoardSize_ThrowsException()
    {
        var random = new Random().Next(1, Board.DefaultSize);
        var invalidBoardSize = Board.DefaultSize - random;

        var board = () => new Board(invalidBoardSize);

        board.Should().Throw<ArgumentException>().WithMessage(ErrorMessages.InvalidBoardSize);
    }

    [Theory]
    [MemberData(nameof(ValidShipPositions))]
    public void AddShip_WhenShipPositionIsValid(
        ShipKind shipKind,
        string bowPosition,
        ShipOrientation orientation
    )
    {
        var board = new Board();

        var shipId = board.AddShip(shipKind, bowPosition, orientation);

        shipId.Should().NotBeEmpty();
        board.Cells.Should().HaveCount(100);
        board.Ships.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(InvalidShipPositions))]
    public void AddShip_WhenShipPositionsIsInvalid(
        ShipKind shipKind,
        string bowPosition,
        ShipOrientation orientation
    )
    {
        var board = new Board();

        var act = () => board.AddShip(shipKind, bowPosition, orientation);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage(ErrorMessages.InvalidShipOnBoardPosition);
        board.Cells.Should().HaveCount(100);
        board.Ships.Should().BeEmpty();
    }

    [Fact]
    public void AddShip_WhenShipPositionIntersectWithOccupiedCells_ThrowsException()
    {
        var board = new Board();
        board.AddShip(ShipKind.Destroyer, "A1", ShipOrientation.Horizontal);

        var act = () => board.AddShip(ShipKind.Battleship, "B1", ShipOrientation.Horizontal);

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidCellToAssign);
    }

    [Fact]
    public void Attack_WhenCellIsValid()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);

        board.Attack("A1"); // Attack the bow of the ship
        board.Attack("A2"); // Attack a clear cell

        board
            .Cells.Where(c => c.State == CellState.Hit)
            .Select(c => c.Code)
            .Should()
            .BeEquivalentTo(["A1", "A2"]);

        board.Cells.Where(c => c.State == CellState.Occupied).Should().HaveCount(2);
        board.Cells.Where(c => c.State == CellState.Clear).Should().HaveCount(96);
    }

    [Fact]
    public void AreAllShipsSunk_WhenAllShipsAreSunk_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);
        board.AddShip(ShipKind.Destroyer, "B1", ShipOrientation.Vertical);

        // Attacking the Battleship
        board.Attack("A1");
        board.Attack("A4");

        // Sinking the Destroyer
        board.Attack("B1");
        board.Attack("B2");

        board.Ships[0].Sunk.Should().BeFalse(); // Battleship
        board.Ships[1].Sunk.Should().BeTrue(); // Destroyer

        board.AreAllShipsSunk.Should().BeFalse();
    }

    [Fact]
    public void AreAllShipsSunk_WhenNotAllShipsAreSunk_ReturnsFalse()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Vertical);
        board.AddShip(ShipKind.Destroyer, "B1", ShipOrientation.Vertical);

        // Sinking the Battleship
        board.Attack("A1");
        board.Attack("A2");
        board.Attack("A3");
        board.Attack("A4");

        // Sinking the Destroyer
        board.Attack("B1");
        board.Attack("B2");

        board.Ships[0].Sunk.Should().BeTrue(); // Battleship
        board.Ships[1].Sunk.Should().BeTrue(); // Destroyer

        board.AreAllShipsSunk.Should().BeTrue();
    }

    [Fact]
    public void IsReady_WhenShipsCountReachAllowance_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        board.AddShip(ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        board.AddShip(ShipKind.Cruiser, "A3", ShipOrientation.Horizontal);
        board.AddShip(ShipKind.Submarine, "A4", ShipOrientation.Horizontal);
        board.AddShip(ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        board.IsReady.Should().BeTrue();
        Board.ShipAllowance.Should().Be(5);
    }

    [Fact]
    public void AddShip_WhenKindAlreadyExists_ThrowsException()
    {
        var board = new Board();
        var shipId = board.AddShip(ShipKind.Carrier, "A1", ShipOrientation.Horizontal);

        var act = () => board.AddShip(ShipKind.Carrier, "A2", ShipOrientation.Horizontal);

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidShipKind);

        board.Ships.Should().ContainSingle(s => s.Id == shipId && s.Kind == ShipKind.Carrier);
    }

    [Fact]
    public void IsReady_WhenShipsCountLessThenMax_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, "A1", ShipOrientation.Horizontal);
        board.AddShip(ShipKind.Destroyer, "A2", ShipOrientation.Horizontal);

        board.IsReady.Should().BeFalse();
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
}
