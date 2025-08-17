using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class CellAttackedEventTests
{
    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreateEvent()
    {
        var gameId = new GameId(Guid.NewGuid());
        const string code = "A1";
        const CellState cellState = CellState.Occupied;

        var cellAttackedEvent = new CellAttackedEvent(gameId, code, cellState);
        cellAttackedEvent.BoardId.Should().Be(gameId);
        cellAttackedEvent.CellCode.Should().Be(code);
        cellAttackedEvent.CellState.Should().Be(cellState);

        cellAttackedEvent.EventId.Should().NotBeEmpty();
        cellAttackedEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cellAttackedEvent.EventType.Should().Be(typeof(CellAttackedEvent));
    }

    [Fact]
    public void Attack_WhenCellIsClear_ShouldRaiseCellAttackedEventWithMissedState()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Act
        game.Attack(BoardSide.Opp, "A1");

        // Assert
        var cellAttackedEvents = game.DomainEvents.OfType<CellAttackedEvent>().ToList();
        cellAttackedEvents.Should().HaveCount(1);

        var cellAttackedEvent = cellAttackedEvents.First();
        cellAttackedEvent.BoardId.Should().Be(game.Id);
        cellAttackedEvent.CellCode.Should().Be("A1");
        cellAttackedEvent.CellState.Should().Be(CellState.Missed);
    }

    [Fact]
    public void Attack_WhenCellIsOccupied_ShouldRaiseCellAttackedEventWithHitState()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Set up a ship on opponent board
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");

        // Act
        game.Attack(BoardSide.Opp, "A1");

        // Assert
        var cellAttackedEvents = game.DomainEvents.OfType<CellAttackedEvent>().ToList();
        cellAttackedEvents.Should().HaveCount(1);

        var cellAttackedEvent = cellAttackedEvents.First();
        cellAttackedEvent.BoardId.Should().Be(game.Id);
        cellAttackedEvent.CellCode.Should().Be("A1");
        cellAttackedEvent.CellState.Should().Be(CellState.Hit);
    }
}
