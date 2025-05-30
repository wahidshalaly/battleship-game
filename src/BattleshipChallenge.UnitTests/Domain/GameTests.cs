using BattleshipChallenge.Domain;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests.Domain;

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
    public void AddShips_WhenShipsCountLessThanAllowance_IsReadyIsFalse()
    {
        var game = new Game();

        game.AddShip(Player.One, ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
        game.IsReady(Player.One).Should().BeFalse();
    }

    [Fact]
    public void AddShips_WhenShipsCountEqualsAllowance_IsReadyIsTrue()
    {
        var game = new Game();

        game.AddShip(Player.One, ShipKind.Carrier, "A1", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Battleship, "A2", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Cruiser, "A3", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Submarine, "A4", ShipOrientation.Horizontal);
        game.AddShip(Player.One, ShipKind.Destroyer, "A5", ShipOrientation.Horizontal);

        game.Id.Should().NotBeEmpty();
        game.State.Should().Be(GameState.Started);
        game.IsReady(Player.One).Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSunk_IsTrue()
    {
        var game = new Game();
        game.AddShip(Player.One, ShipKind.Cruiser, "A1", ShipOrientation.Vertical);
        game.Attack(Player.One, "A1");
        game.Attack(Player.One, "A2");
        game.Attack(Player.One, "A3");

        game.IsGameOver(Player.One).Should().BeTrue();
    }

    //[Fact]
    //public void Attack_WhenNotOccupied_ReturnFalse()
    //{
    //    var game = new Game();
    //    game.Attack(Player.One, "C5");

    //    Assert.Fail();
    //}

    //[Fact]
    //public void Attack_WhenOccupied_ReturnTrue()
    //{
    //    var game = new Game();
    //    game.AddShip(Player.One, ShipKind.Cruiser, "C3", isVertical: false);
    //    game.Attack(Player.One, "C4");

    //    Assert.Fail();
    //}

    //[Fact]
    //public void PlayerHasLost_WhenShipsSunk_ReturnTrue()
    //{
    //    var game = new Game();
    //    game.CreateBoard(Player.One);
    //    game.AddShip(Player.One, (Cell)"C3", (Cell)"C5");
    //    game.Attack(Player.One, (Cell)"C3");
    //    game.Attack(Player.One, (Cell)"C4");
    //    game.Attack(Player.One, (Cell)"C5");

    //    var result = game.PlayerHasLost(Player.One);

    //    Assert.True(result);
    //}
}
