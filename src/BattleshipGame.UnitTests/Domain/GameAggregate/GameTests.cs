using System;
using BattleshipGame.Common;
using BattleshipGame.Domain;
using BattleshipGame.Domain.GameAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.GameAggregate;

public class GameTests
{
    [Fact]
    public void Ctor_CreateBoardWithDefaultSize()
    {
        var game = new Game();

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
    }

    [Fact]
    public void AddShips_WhenCountLessThanAllowance_IsReadyIsFalse()
    {
        var game = new Game();

        game.AddShip(BoardSide.Own, ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeFalse();
    }

    [Fact]
    public void AddShips_WhenCountEqualsAllowance_IsReadyIsTrue()
    {
        var game = new Game();

        game.AddShip(BoardSide.Own, ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, "A3", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Submarine, "A4", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeTrue();
    }

    [Fact]
    public void AddShips_WhenExceedAllowance_ThrowsException()
    {
        var game = new Game();
        game.AddShip(BoardSide.Own, ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, "A3", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Submarine, "A4", ShipOrientation.Horizontal);
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        Action act = () =>
            game.AddShip(BoardSide.Own, ShipKind.Destroyer, "A6", ShipOrientation.Horizontal);

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidShipKind);

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSunk_IsTrue()
    {
        var game = new Game();
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, "A1", ShipOrientation.Vertical);
        game.Attack(BoardSide.Own, "A1");
        game.Attack(BoardSide.Own, "A2");
        game.Attack(BoardSide.Own, "A3");

        game.IsGameOver(BoardSide.Own).Should().BeTrue();
    }
}
