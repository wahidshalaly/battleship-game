using System;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
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
        game.State.Should().Be(GameState.New);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(26)]
    public void Ctor_CreateBoardWithCustomSize(int boardSize)
    {
        var game = new Game(_playerId, boardSize);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.New);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(27)]
    [InlineData(35)]
    public void Ctor_WithInvalidBoardSize_ShouldThrowArgumentException(int boardSize)
    {
        var act = () => new Game(_playerId, boardSize);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void PlaceShips_WhenCountLessThanAllowance_IsReadyIsFalse(BoardSide boardSide)
    {
        var game = _fixture.GetNewGame(_playerId);

        game.PlaceShip(boardSide, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.PlaceShip(boardSide, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.PlaceShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.New);
        game.IsBoardReady(boardSide).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void PlaceShips_WhenCountEqualsAllowance_IsReadyIsTrue(BoardSide boardSide)
    {
        var game = new Game(_playerId);

        game.PlaceShip(boardSide, ShipKind.Carrier, ShipOrientation.Horizontal, "A1");
        game.PlaceShip(boardSide, ShipKind.Battleship, ShipOrientation.Horizontal, "A2");
        game.PlaceShip(boardSide, ShipKind.Cruiser, ShipOrientation.Horizontal, "A3");
        game.PlaceShip(boardSide, ShipKind.Submarine, ShipOrientation.Horizontal, "A4");
        game.PlaceShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A5");

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.New);
        game.IsBoardReady(boardSide).Should().BeTrue();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void PlaceShips_WhenExceedAllowance_ThrowsException(BoardSide boardSide)
    {
        var game = _fixture.GetReadyGame(_playerId);

        Action act = () =>
            game.PlaceShip(boardSide, ShipKind.Destroyer, ShipOrientation.Horizontal, "A6");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ErrorMessages.InvalidShipKindAlreadyExists);

        game.Id.Should().NotBe(Guid.Empty);
        game.State.Should().Be(GameState.Ready);
        game.IsBoardReady(boardSide).Should().BeTrue();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void IsGameOver_WhenAllShipsSunk_IsTrue(BoardSide winnerSide)
    {
        var game = _fixture.GetCompletedGame(_playerId, winnerSide);
        var defeatedSide = winnerSide.OppositeSide();

        game.IsGameOver(defeatedSide).Should().BeTrue();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void PlaceShip_WhenValidParameters_ShouldReturnShipId(BoardSide boardSide)
    {
        var game = _fixture.GetNewGame(_playerId);

        var shipId = game.PlaceShip(
            boardSide,
            ShipKind.Destroyer,
            ShipOrientation.Horizontal,
            "A1"
        );

        shipId.Should().NotBeNull();
        shipId.Value.Should().NotBeEmpty();
    }

    // TODO: Add more tests for PlaceShip method (invalid placements, overlapping ships, etc.)

    [Fact]
    public void Attack_WhenValidCell_ShouldNotThrow()
    {
        var game = _fixture.GetStartedGame(_playerId);

        var act = () => game.Attack(BoardSide.Opponent, "A1");

        act.Should().NotThrow();
    }

    [Fact]
    public void Attack_WhenPreviouslyAttacked_ShouldThrowException()
    {
        var game = _fixture.GetStartedGame(_playerId);
        game.Attack(BoardSide.Opponent, "A1");
        game.Attack(BoardSide.Player, "A1");
        game.Attack(BoardSide.Opponent, "A2");

        var act = () => game.Attack(BoardSide.Player, "A1");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ErrorMessages.InvalidCellToAttack);
    }

    [Fact]
    public void IsReady_WhenBothBoardsReady_ShouldReturnTrueForBoth()
    {
        var game = _fixture.GetReadyGame();

        game.IsBoardReady(BoardSide.Player).Should().BeTrue();
        game.IsBoardReady(BoardSide.Opponent).Should().BeTrue();
        game.State.Should().Be(GameState.Ready);

        game.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void IsGameOver_WhenNoShipsAttacked_ShouldBeFalse()
    {
        var game = _fixture.GetNewGame(_playerId);

        game.IsGameOver(BoardSide.Player).Should().BeFalse();
        game.IsGameOver(BoardSide.Opponent).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void IsGameOver_WhenBoardIsLost_ShouldBeTrue(BoardSide winnerSide)
    {
        var game = _fixture.GetCompletedGame(_playerId, winnerSide);

        game.IsGameOver(winnerSide).Should().BeFalse();
        game.IsGameOver(winnerSide.OppositeSide()).Should().BeTrue();
    }

    [Fact]
    public void GetShips_WhenBoardsAreEmpty_ShouldReturnNothing()
    {
        var game = _fixture.GetNewGame(_playerId);
        game.GetShips(BoardSide.Player).Should().BeEmpty();
        game.GetShips(BoardSide.Opponent).Should().BeEmpty();
    }

    [Fact]
    public void GetShips_WhenGameReady_ShouldReturnShips()
    {
        var game = _fixture.GetReadyGame();
        game.GetShips(BoardSide.Player).Should().HaveCount(ShipAllowance);
        game.GetShips(BoardSide.Opponent).Should().HaveCount(ShipAllowance);
        game.State.Should().Be(GameState.Ready);
    }

    [Fact]
    public void StartGameplay_WhenGameIsNotReady_ThrowsException()
    {
        var game = _fixture.GetNewGame();
        var act = game.StartGameplay;

        act.Should().Throw<GameNotReadyException>();
    }

    [Fact]
    public void StartGameplay_WhenGameIsReady_Returns()
    {
        var game = _fixture.GetReadyGame();
        game.StartGameplay();
        game.State.Should().Be(GameState.Started);
    }

    [Fact]
    public void StartGameplay_WhenGameIsReady_RaisesGameStartedEvent()
    {
        var game = _fixture.GetReadyGame();
        game.StartGameplay();
        game.DomainEvents.Should().ContainSingle(e => e is GameStartedEvent);
    }
}
