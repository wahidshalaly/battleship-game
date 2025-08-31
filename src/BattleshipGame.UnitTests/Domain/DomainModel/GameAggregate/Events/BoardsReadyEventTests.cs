using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class BoardsReadyEventTests
{
    [Fact]
    public void AddShip_WhenBothBoardsReady_ShouldRaiseBoardsReadyEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Act - Add ships to both boards to make them ready (using different positions)
        game.AddShip(BoardSide.Own, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(BoardSide.Own, ShipKind.Cruiser, ShipOrientation.Vertical, "B1");
        game.AddShip(BoardSide.Own, ShipKind.Destroyer, ShipOrientation.Vertical, "C1");
        game.AddShip(BoardSide.Own, ShipKind.Submarine, ShipOrientation.Vertical, "D1");
        game.AddShip(BoardSide.Own, ShipKind.Carrier, ShipOrientation.Vertical, "E1");

        game.AddShip(BoardSide.Opp, ShipKind.Battleship, ShipOrientation.Vertical, "F1");
        game.AddShip(BoardSide.Opp, ShipKind.Cruiser, ShipOrientation.Vertical, "G1");
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "H1");
        game.AddShip(BoardSide.Opp, ShipKind.Submarine, ShipOrientation.Vertical, "I1");
        game.AddShip(BoardSide.Opp, ShipKind.Carrier, ShipOrientation.Vertical, "J1");

        // Assert
        game.IsReady.Should().BeTrue();
        game.State.Should().Be(GameState.BoardsAreReady);

        var boardsReadyEvents = game.DomainEvents.OfType<BoardsReadyEvent>().ToList();
        boardsReadyEvents.Should().HaveCount(1);

        var boardsReadyEvent = boardsReadyEvents.First();
        boardsReadyEvent.GameId.Should().Be(game.Id);
    }
}
