using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class BoardSideReadyEventTests
{
    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Ctor_WhenValidParameters_ShouldCreateEvent(BoardSide boardSide)
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());

        // Act
        var boardSideReadyEvent = new BoardSideReadyEvent(gameId, boardSide);

        // Assert
        boardSideReadyEvent.GameId.Should().Be(gameId);
        boardSideReadyEvent.BoardSide.Should().Be(boardSide);
        boardSideReadyEvent.EventId.Should().NotBeEmpty();
        boardSideReadyEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        boardSideReadyEvent.EventType.Should().Be(typeof(BoardSideReadyEvent));
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void AddShip_WhenBoardSideBecomesReady_ShouldRaiseBoardSideReadyEvent(BoardSide boardSide)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Act - Add all required ships to make the specified board side ready
        game.AddShip(boardSide, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(boardSide, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");
        game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Vertical, "E1");
        game.AddShip(boardSide, ShipKind.Submarine, ShipOrientation.Vertical, "G1");
        game.AddShip(boardSide, ShipKind.Carrier, ShipOrientation.Vertical, "I1"); // This should trigger the event

        // Assert
        var boardSideReadyEvents = game.DomainEvents.OfType<BoardSideReadyEvent>().ToList();
        boardSideReadyEvents.Should().HaveCount(1);

        var boardSideReadyEvent = boardSideReadyEvents.First();
        boardSideReadyEvent.GameId.Should().Be(game.Id);
        boardSideReadyEvent.BoardSide.Should().Be(boardSide);
        boardSideReadyEvent.EventId.Should().NotBeEmpty();
        boardSideReadyEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        boardSideReadyEvent.EventType.Should().Be(typeof(BoardSideReadyEvent));
    }

    [Fact]
    public void AddShip_WhenPartialShipsAdded_ShouldNotRaiseBoardSideReadyEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Act - Add only some ships (not all required)
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Vertical, "E1");
        // Missing Submarine and Carrier

        // Assert
        var boardSideReadyEvents = game.DomainEvents.OfType<BoardSideReadyEvent>().ToList();
        boardSideReadyEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddShip_WhenBothBoardSidesReady_ShouldRaiseTwoBoardSideReadyEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Act - Make Own side ready first
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Vertical, "E1");
        game.AddShip(BoardSide.Own, ShipKind.Submarine, ShipOrientation.Vertical, "G1");
        game.AddShip(BoardSide.Own, ShipKind.Carrier, ShipOrientation.Vertical, "I1");

        // Then make Opp side ready
        game.AddShip(BoardSide.Opp, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(BoardSide.Opp, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "E1");
        game.AddShip(BoardSide.Opp, ShipKind.Submarine, ShipOrientation.Vertical, "G1");
        game.AddShip(BoardSide.Opp, ShipKind.Carrier, ShipOrientation.Vertical, "I1");

        // Assert
        var boardSideReadyEvents = game.DomainEvents.OfType<BoardSideReadyEvent>().ToList();
        boardSideReadyEvents.Should().HaveCount(2);

        // Should have one event for each board side
        boardSideReadyEvents.Should().Contain(e => e.BoardSide == BoardSide.Own);
        boardSideReadyEvents.Should().Contain(e => e.BoardSide == BoardSide.Opp);
        boardSideReadyEvents.Should().OnlyContain(e => e.GameId == game.Id);
    }
}
