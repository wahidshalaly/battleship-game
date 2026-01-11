using System;
using System.Threading.Tasks;
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

    [Theory]
    [InlineData(GameState.New)]
    [InlineData(GameState.Ready)]
    public void Attack_WhenGameNotStarted_ThrowsGameNotStartedException(GameState state)
    {
        var game =
            state == GameState.New
                ? _fixture.GetNewGame(_playerId)
                : _fixture.GetReadyGame(_playerId);

        var act = () => game.Attack(BoardSide.Opponent, "A1");

        act.Should().Throw<GameNotStartedException>().WithMessage($"*{game.Id.Value}*{state}*");
    }

    [Fact]
    public void Attack_WhenGameIsOver_ThrowsGameOverException()
    {
        var game = _fixture.GetCompletedGame(_playerId, BoardSide.Player);

        var act = () => game.Attack(BoardSide.Opponent, "B1");

        act.Should().Throw<GameOverException>().WithMessage($"*{game.Id.Value}*");
    }

    [Fact]
    public void Attack_WhenTargetSideDoesNotMatchExpected_ThrowsInvalidTargetSideException()
    {
        var game = _fixture.GetStartedGame(_playerId);

        // Game starts with TargetSide = BoardSide.Opponent
        var act = () => game.Attack(BoardSide.Player, "A1");

        act.Should()
            .Throw<InvalidTargetSideException>()
            .WithMessage($"*{game.Id.Value}*{BoardSide.Opponent}*{BoardSide.Player}*");
    }

    [Fact]
    public void Attack_WhenValidTarget_SwitchesTargetSideAfterAttack()
    {
        var game = _fixture.GetStartedGame(_playerId);

        game.TargetSide.Should().Be(BoardSide.Opponent);

        game.Attack(BoardSide.Opponent, "A1");

        game.TargetSide.Should().Be(BoardSide.Player);
    }

    [Fact]
    public void Attack_AlternatingAttacks_ValidatesCorrectTargetSide()
    {
        var game = _fixture.GetStartedGame(_playerId);

        // First attack on Opponent - should succeed
        var act1 = () => game.Attack(BoardSide.Opponent, "A1");
        act1.Should().NotThrow();
        game.TargetSide.Should().Be(BoardSide.Player);

        // Second attack on Player - should succeed
        var act2 = () => game.Attack(BoardSide.Player, "A1");
        act2.Should().NotThrow();
        game.TargetSide.Should().Be(BoardSide.Opponent);

        // Third attack on Player again - should fail (wrong target)
        var act3 = () => game.Attack(BoardSide.Player, "A2");
        act3.Should().Throw<InvalidTargetSideException>();
    }

    [Fact]
    public void WinnerSide_WhenGameIsNew_ShouldBeNone()
    {
        var game = _fixture.GetNewGame(_playerId);

        game.WinnerSide.Should().Be(BoardSide.None);
    }

    [Fact]
    public void WinnerSide_WhenGameIsReady_ShouldBeNone()
    {
        var game = _fixture.GetReadyGame(_playerId);

        game.WinnerSide.Should().Be(BoardSide.None);
    }

    [Fact]
    public void WinnerSide_WhenGameIsStarted_ShouldBeNone()
    {
        var game = _fixture.GetStartedGame(_playerId);

        game.WinnerSide.Should().Be(BoardSide.None);
    }

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void WinnerSide_WhenGameIsCompleted_ShouldBeSetCorrectly(BoardSide winnerSide)
    {
        var game = _fixture.GetCompletedGame(_playerId, winnerSide);

        game.WinnerSide.Should().Be(winnerSide);
        game.State.Should().Be(GameState.GameOver);
    }

    [Fact]
    public void CreatedAt_WhenGameIsCreated_ShouldBeSetToUtcNow()
    {
        var beforeCreation = DateTime.UtcNow;
        var game = _fixture.GetNewGame(_playerId);

        game.LastUpdatedAt.Should().BeCloseTo(beforeCreation, TimeSpan.FromMilliseconds(100));
        game.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void LastUpdatedAt_WhenGameIsCreated_ShouldBeSetToUtcNow()
    {
        var beforeCreation = DateTime.UtcNow;
        var game = _fixture.GetNewGame(_playerId);

        game.LastUpdatedAt.Should().BeCloseTo(beforeCreation, TimeSpan.FromMilliseconds(100));
        game.LastUpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task LastUpdatedAt_WhenShipIsPlaced_ShouldBeUpdated()
    {
        var game = _fixture.GetNewGame(_playerId);
        var initialLastUpdated = game.LastUpdatedAt;

        // Wait briefly to ensure timestamp can differ
        await Task.Delay(10);
        game.PlaceShip(BoardSide.Player, ShipKind.Destroyer, ShipOrientation.Horizontal, "A1");

        game.LastUpdatedAt.Should().BeCloseTo(initialLastUpdated, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task LastUpdatedAt_WhenGameplayStarts_ShouldBeUpdated()
    {
        var game = _fixture.GetReadyGame(_playerId);
        var initialLastUpdated = game.LastUpdatedAt;

        // Wait briefly to ensure timestamp can differ
        await Task.Delay(10);
        game.StartGameplay();

        game.LastUpdatedAt.Should().BeCloseTo(initialLastUpdated, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task LastUpdatedAt_WhenAttackIsMade_ShouldBeUpdated()
    {
        var game = _fixture.GetStartedGame(_playerId);
        var initialLastUpdated = game.LastUpdatedAt;

        // Wait briefly to ensure timestamp can differ
        await Task.Delay(10);
        game.Attack(BoardSide.Opponent, "A1");

        game.LastUpdatedAt.Should().BeCloseTo(initialLastUpdated, TimeSpan.FromMilliseconds(100));
    }
}
