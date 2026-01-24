using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class UnderAttackEventTests
{
    private readonly GameFixture _gameFixture = new();

    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreateEvent()
    {
        var gameId = new GameId(Guid.NewGuid());
        const BoardSide boardSide = BoardSide.Player;
        const string code = "A1";
        const CellState cellState = CellState.Occupied;

        var cellAttackedEvent = new UnderAttackEvent(gameId, boardSide, code, cellState);
        cellAttackedEvent.GameId.Should().Be(gameId);
        cellAttackedEvent.BoardSide.Should().Be(boardSide);
        cellAttackedEvent.CellCode.Should().Be(code);
        cellAttackedEvent.CellState.Should().Be(cellState);

        cellAttackedEvent.EventId.Should().NotBeEmpty();
        cellAttackedEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cellAttackedEvent.EventType.Should().Be(typeof(UnderAttackEvent));
    }

    [Fact]
    public void Attack_WhenCellIsClear_ShouldRaiseCellAttackedEventWithMissedState()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var clearCell = game.GetNextTargets(BoardSide.Opponent).Last();

        // Act
        game.Attack(BoardSide.Opponent, clearCell);

        // Assert
        var cellAttackedEvents = game.DomainEvents.OfType<UnderAttackEvent>().ToList();
        cellAttackedEvents.Should().HaveCount(1);

        var cellAttackedEvent = cellAttackedEvents.First();
        cellAttackedEvent.GameId.Should().Be(game.Id);
        cellAttackedEvent.BoardSide.Should().Be(BoardSide.Opponent);
        cellAttackedEvent.CellCode.Should().Be(clearCell);
        cellAttackedEvent.CellState.Should().Be(CellState.Missed);
    }

    [Fact]
    public void Attack_WhenCellIsOccupied_ShouldRaiseCellAttackedEventWithHitState()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var occupiedCell = game.GetNextTargets(BoardSide.Opponent).First();

        // Act
        game.Attack(BoardSide.Opponent, occupiedCell);

        // Assert
        var cellAttackedEvents = game.DomainEvents.OfType<UnderAttackEvent>().ToList();
        cellAttackedEvents.Should().HaveCount(1);

        var cellAttackedEvent = cellAttackedEvents.First();
        cellAttackedEvent.GameId.Should().Be(game.Id);
        cellAttackedEvent.BoardSide.Should().Be(BoardSide.Opponent);
        cellAttackedEvent.CellCode.Should().Be(occupiedCell);
        cellAttackedEvent.CellState.Should().Be(CellState.Hit);
    }
}
