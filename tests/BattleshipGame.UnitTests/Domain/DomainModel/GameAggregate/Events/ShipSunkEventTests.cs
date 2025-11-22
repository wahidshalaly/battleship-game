using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class ShipSunkEventTests
{
    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Ctor_WhenValidParameters_ShouldCreateEvent(BoardSide attackedSide)
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        var shipId = new ShipId(Guid.NewGuid());

        // Act
        var shipSunkEvent = new ShipSunkEvent(gameId, shipId, attackedSide);

        // Assert
        shipSunkEvent.GameId.Should().Be(gameId);
        shipSunkEvent.ShipId.Should().Be(shipId);
        shipSunkEvent.AttackedSide.Should().Be(attackedSide);
        shipSunkEvent.EventId.Should().NotBeEmpty();
        shipSunkEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        shipSunkEvent.EventType.Should().Be(typeof(ShipSunkEvent));
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Attack_WhenDestroyerSunk_ShouldRaiseShipSunkEvent(BoardSide attackedSide)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Add a Destroyer (2 cells) to the attacked side
        game.AddShip(attackedSide, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");

        // Act - Attack all cells of the destroyer to sink it
        game.Attack(attackedSide, "A1"); // Hit first cell
        game.Attack(attackedSide, "A2"); // Hit second cell and sink the ship

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().HaveCount(1);

        var shipSunkEvent = shipSunkEvents.First();
        shipSunkEvent.GameId.Should().Be(game.Id);
        shipSunkEvent.AttackedSide.Should().Be(attackedSide);
        shipSunkEvent.ShipId.Should().NotBeNull();
        shipSunkEvent.EventId.Should().NotBeEmpty();
        shipSunkEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        shipSunkEvent.EventType.Should().Be(typeof(ShipSunkEvent));
    }

    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Attack_WhenCarrierSunk_ShouldRaiseShipSunkEvent(BoardSide attackedSide)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Add a Carrier (5 cells) to the attacked side
        game.AddShip(attackedSide, ShipKind.Carrier, ShipOrientation.Vertical, "A1");

        // Act - Attack all cells of the carrier to sink it
        game.Attack(attackedSide, "A1"); // Hit first cell
        game.Attack(attackedSide, "A2"); // Hit second cell
        game.Attack(attackedSide, "A3"); // Hit third cell
        game.Attack(attackedSide, "A4"); // Hit fourth cell
        game.Attack(attackedSide, "A5"); // Hit fifth cell and sink the ship

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().HaveCount(1);

        var shipSunkEvent = shipSunkEvents.First();
        shipSunkEvent.GameId.Should().Be(game.Id);
        shipSunkEvent.AttackedSide.Should().Be(attackedSide);
        shipSunkEvent.ShipId.Should().NotBeNull();
    }

    [Fact]
    public void Attack_WhenShipPartiallyHit_ShouldNotRaiseShipSunkEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Add a Destroyer (2 cells) to opponent board
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");

        // Act - Attack only one cell of the destroyer (not sinking it)
        game.Attack(BoardSide.Opp, "A1"); // Hit first cell only

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().BeEmpty();
    }

    [Fact]
    public void Attack_WhenMultipleShipsSunk_ShouldRaiseMultipleShipSunkEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Add two Destroyers to opponent board
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");
        game.AddShip(BoardSide.Opp, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");

        // Act - Sink both destroyers
        // Sink first destroyer
        game.Attack(BoardSide.Opp, "A1");
        game.Attack(BoardSide.Opp, "A2");

        // Sink second destroyer
        game.Attack(BoardSide.Opp, "C1");
        game.Attack(BoardSide.Opp, "C2");
        game.Attack(BoardSide.Opp, "C3");

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().HaveCount(2);

        // All events should be for the same game and attacked side
        shipSunkEvents.Should().OnlyContain(e => e.GameId == game.Id);
        shipSunkEvents.Should().OnlyContain(e => e.AttackedSide == BoardSide.Opp);

        // Each event should have different ship IDs
        var shipIds = shipSunkEvents.Select(e => e.ShipId).ToList();
        shipIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Attack_WhenMissedAttack_ShouldNotRaiseShipSunkEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Add a ship to opponent board but attack empty cell
        game.AddShip(BoardSide.Opp, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");

        // Act - Attack empty cell
        game.Attack(BoardSide.Opp, "B1");

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().BeEmpty();
    }
}
