using System;
using System.Linq;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

public class GameTests
{
    private readonly GameFixture _fixture = new();
    private readonly PlayerId _playerId = new(Guid.NewGuid());

    [Fact]
    public void Ctor_CreateBoardWithDefaultSize()
    {
        var game = new Game(_playerId);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void AddShips_WhenCountLessThanAllowance_IsReadyIsFalse(BoardSide boardSide)
    {
        var game = _fixture.CreateNewGame(_playerId);

        game.AddShip(boardSide, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(boardSide, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsBoardReady(boardSide).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void AddShips_WhenCountEqualsAllowance_IsReadyIsTrue(BoardSide boardSide)
    {
        var game = new Game(_playerId);

        game.AddShip(boardSide, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(boardSide, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(boardSide, ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        game.AddShip(boardSide, ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsBoardReady(boardSide).Should().BeTrue();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void AddShips_WhenExceedAllowance_ThrowsException(BoardSide boardSide)
    {
        var game = new Game(_playerId);
        game.AddShip(boardSide, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.AddShip(boardSide, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.AddShip(boardSide, ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        game.AddShip(boardSide, ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        Action act = () =>
            game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A6");

        act.Should().Throw<InvalidOperationException>().WithMessage(ErrorMessages.InvalidShipKindAlreadyExists);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Started);
        game.IsBoardReady(boardSide).Should().BeTrue();
    }

    [Fact]
    public void IsGameOver_WhenAllShipsSunk_IsTrue()
    {
        var game = _fixture.CreateNewGame(_playerId);
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Vertical, "A1");
        game.Attack(BoardSide.Own, "A1");
        game.Attack(BoardSide.Own, "A2");
        game.Attack(BoardSide.Own, "A3");

        game.IsGameOver(BoardSide.Own).Should().BeTrue();
    }

    [Fact]
    public void Ctor_WhenCustomBoardSize_ShouldCreateGameWithSpecifiedSize()
    {
        const int customSize = 15;
        var game = _fixture.CreateNewGame(_playerId, customSize);

        game.BoardSize.Should().Be(customSize);
        game.PlayerId.Should().Be(_playerId);
        game.State.Should().Be(GameState.Started);
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void AddShip_WhenValidParameters_ShouldReturnShipId(BoardSide boardSide)
    {
        var game = _fixture.CreateNewGame(_playerId);

        var shipId = game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A1");

        shipId.Should().NotBeNull();
        shipId.Value.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Attack_WhenValidCell_ShouldNotThrow(BoardSide boardSide)
    {
        var game = _fixture.CreateNewGame(_playerId);

        var act = () => game.Attack(boardSide, "A1");

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Attack_WhenSameCell_ShouldThrowException(BoardSide boardSide)
    {
        var game = _fixture.CreateNewGame(_playerId);
        game.Attack(boardSide, "A1");

        var act = () => game.Attack(boardSide, "A1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ErrorMessages.InvalidCellToAttack);
    }

    [Fact]
    public void IsReady_WhenBothBoardsReady_ShouldReturnTrueForBoth()
    {
        var game = _fixture.CreateReadyGame();
       
        game.IsBoardReady(BoardSide.Own).Should().BeTrue();
        game.IsBoardReady(BoardSide.Opp).Should().BeTrue();
        game.State.Should().Be(GameState.BoardsAreReady);

        game.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void IsGameOver_WhenNoShipsAttacked_ShouldBeFalse()
    {
        var game = _fixture.CreateNewGame(_playerId);

        game.IsGameOver(BoardSide.Own).Should().BeFalse();
        game.IsGameOver(BoardSide.Opp).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void IsGameOver_WhenPartiallyAttacked_ShouldBeFalse(BoardSide boardSide)
    {
        var game = _fixture.CreateReadyGame(_playerId);
        var shipId = game.GetShips(boardSide).First();
        _fixture.AttackShip(game, boardSide, shipId);

        game.IsGameOver(boardSide).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void IsGameOver_WhenBoardIsLost_ShouldBeTrue(BoardSide winnerSide)
    {
        var game = _fixture.CreateCompletedGame(_playerId, winnerSide);

        game.IsGameOver(winnerSide).Should().BeFalse();
        game.IsGameOver(winnerSide.OppositeSide()).Should().BeTrue();
    }

    [Fact]
    public void GetShips_WhenBoardsAreEmpty_ShouldReturnNothing()
    {
        var game = _fixture.CreateNewGame(_playerId);
        game.GetShips(BoardSide.Own).Should().BeEmpty();
        game.GetShips(BoardSide.Opp).Should().BeEmpty();
    }

    [Fact]
    public void GetShips_WhenBoardsAreReady_ShouldReturnShips()
    {
        var game = _fixture.CreateReadyGame();
        game.GetShips(BoardSide.Own).Should().HaveCount(ShipAllowance);
        game.GetShips(BoardSide.Opp).Should().HaveCount(ShipAllowance);
    }
}
