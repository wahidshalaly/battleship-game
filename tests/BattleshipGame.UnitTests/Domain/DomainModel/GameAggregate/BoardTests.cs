using System;
using System.Linq;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using FluentAssertions;
using Xunit;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

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
        var random = new Random().Next(1, DefaultBoardSize);
        var invalidBoardSize = DefaultBoardSize - random;

        var board = () => new Board(invalidBoardSize);

        board.Should().Throw<ArgumentException>().WithMessage(ErrorMessages.InvalidBoardSize);
    }

    [Theory]
    [MemberData(nameof(ValidShipPositions))]
    public void AddShip_WhenShipPositionIsValid(
        ShipKind shipKind,
        string bowPosition,
        ShipOrientation orientation,
        string[] expectedPosition
    )
    {
        var board = new Board();

        var shipId = board.AddShip(shipKind, orientation, bowPosition);

        shipId.Should().NotBe(Guid.Empty);
        board.Ships.Should().HaveCount(1);
        board.Ships[0].Position.Should().BeEquivalentTo(expectedPosition);
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

        var act = () => board.AddShip(shipKind, orientation, bowPosition);

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
        board.AddShip(ShipKind.Destroyer, ShipOrientation.Horizontal, "A1");

        var act = () => board.AddShip(ShipKind.Battleship, ShipOrientation.Horizontal, "B1");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ErrorMessages.InvalidCellToAssign);
    }

    [Fact]
    public void Attack_WhenCellIsValid()
    {
        // Arrange
        var board = new Board();
        board.AddShip(ShipKind.Battleship, ShipOrientation.Horizontal, "A1");

        // Act
        board.Attack("A1"); // Attack the bow of the ship
        board.Attack("A2"); // Attack a clear cell

        // Assert
        board.Cells.Where(c => c.State == CellState.Hit).Should().HaveCount(1);
        board.Cells.Where(c => c.State == CellState.Missed).Should().HaveCount(1);
        board.Cells.Where(c => c.State == CellState.Occupied).Should().HaveCount(3);
        board.Cells.Where(c => c.State == CellState.Clear).Should().HaveCount(95);

        ValidateState(CellState.Hit, ["A1"]);
        ValidateState(CellState.Missed, ["A2"]);
        ValidateState(CellState.Occupied, ["B1", "C1", "D1"]);

        return;

        void ValidateState(CellState state, string[] expectedCodes)
        {
            board
                .Cells.Where(c => c.State == state)
                .Select(c => c.Code)
                .Should()
                .BeEquivalentTo(expectedCodes);
        }
    }

    [Fact]
    public void IsGameOver_WhenNotAllShipsAreSunk_ReturnsFalse()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        board.AddShip(ShipKind.Destroyer, ShipOrientation.Vertical, "B1");

        // Attacking the Battleship
        board.Attack("A1");
        board.Attack("A4");

        // Sinking the Destroyer
        board.Attack("B1");
        board.Attack("B2");

        board.Ships[0].Sunk.Should().BeFalse(); // Battleship
        board.Ships[1].Sunk.Should().BeTrue(); // Destroyer

        board.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsAreSunk_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        board.AddShip(ShipKind.Destroyer, ShipOrientation.Vertical, "B1");

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

        board.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void IsReady_WhenShipsCountReachAllowance_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        board.AddShip(ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        board.AddShip(ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        board.AddShip(ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        board.AddShip(ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        board.IsReady.Should().BeTrue();
        ShipAllowance.Should().Be(5);
    }

    [Fact]
    public void AddShip_WhenKindAlreadyExists_ThrowsException()
    {
        var board = new Board();
        var shipId = board.AddShip(ShipKind.Carrier, ShipOrientation.Horizontal, "A1");

        var act = () => board.AddShip(ShipKind.Carrier, ShipOrientation.Horizontal, "A2");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ErrorMessages.InvalidShipKindAlreadyExists);

        board.Ships.Should().ContainSingle(s => s.Id == shipId && s.Kind == ShipKind.Carrier);
    }

    [Fact]
    public void IsReady_WhenShipsCountLessThenMax_ReturnsTrue()
    {
        var board = new Board();
        board.AddShip(ShipKind.Battleship, ShipOrientation.Horizontal, "A1");
        board.AddShip(ShipKind.Destroyer, ShipOrientation.Horizontal, "A2");

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

    public static TheoryData<ShipKind, string, ShipOrientation, string[]> ValidShipPositions =>
        new()
        {
            { ShipKind.Destroyer, "A1", ShipOrientation.Vertical, ["A1", "A2"] },
            { ShipKind.Submarine, "B2", ShipOrientation.Horizontal, ["B2", "C2", "D2"] },
            { ShipKind.Cruiser, "C3", ShipOrientation.Vertical, ["C3", "C4", "C5"] },
            { ShipKind.Battleship, "D4", ShipOrientation.Horizontal, ["D4", "E4", "F4", "G4"] },
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
