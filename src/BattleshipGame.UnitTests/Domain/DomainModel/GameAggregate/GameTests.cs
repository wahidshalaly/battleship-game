using System;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

public class GameTests
{
    private readonly PlayerId _playerId = new PlayerId(Guid.NewGuid());

    [Fact]
    public void Ctor_CreateBoardWithDefaultSize()
    {
        var game = new Game(_playerId);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
    }

    [Fact]
    public void AddShips_WhenCountLessThanAllowance_IsReadyIsFalse()
    {
        var game = new Game(_playerId);

        game.AddShip(BoardSide.Own, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeFalse();
    }

    [Fact]
    public void AddShips_WhenCountEqualsAllowance_IsReadyIsTrue()
    {
        var game = new Game(_playerId);

        game.AddShip(BoardSide.Own, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        game.AddShip(BoardSide.Own, ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeTrue();
    }

    [Fact]
    public void AddShips_WhenExceedAllowance_ThrowsException()
    {
        var game = new Game(_playerId);
        game.AddShip(BoardSide.Own, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        game.AddShip(BoardSide.Own, ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        Action act = () =>
            game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Horizontal, "A6");

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidShipKind);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsReady(BoardSide.Own).Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSunk_IsTrue()
    {
        var game = new Game(_playerId);
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Vertical, "A1");
        game.Attack(BoardSide.Own, "A1");
        game.Attack(BoardSide.Own, "A2");
        game.Attack(BoardSide.Own, "A3");

        game.IsGameOver(BoardSide.Own).Should().BeTrue();
    }
}
