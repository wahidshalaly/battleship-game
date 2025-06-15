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

    //[Fact]
    //public void Attack_WhenNotOccupied_ReturnFalse()
    //{
    //    var game = new Game();
    //    game.Attack(BoardSide.Own, "C5");

    //    Assert.Fail();
    //}

    //[Fact]
    //public void Attack_WhenOccupied_ReturnTrue()
    //{
    //    var game = new Game();
    //    game.AddShip(BoardSide.Own, ShipKind.Cruiser, "C3", isVertical: false);
    //    game.Attack(BoardSide.Own, "C4");

    //    Assert.Fail();
    //}

    //[Fact]
    //public void PlayerHasLost_WhenShipsSunk_ReturnTrue()
    //{
    //    var game = new Game();
    //    game.CreateBoard(BoardSide.Own);
    //    game.AddShip(BoardSide.Own, (Cell)"C3", (Cell)"C5");
    //    game.Attack(BoardSide.Own, (Cell)"C3");
    //    game.Attack(BoardSide.Own, (Cell)"C4");
    //    game.Attack(BoardSide.Own, (Cell)"C5");

    //    var result = game.PlayerHasLost(BoardSide.Own);

    //    Assert.True(result);
    //}
}
