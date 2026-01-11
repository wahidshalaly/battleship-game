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
    private readonly GameFixture _fixture = new();

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
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
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void Attack_WhenShipSunk_ShouldRaiseShipSunkEvent(BoardSide targetSide)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = _fixture.CreateGameInStateStarted(playerId);
        var shipId = game.GetShips(targetSide).First();

        // Act - Attack all cells of ship to sink it
        _fixture.AttackShip(game, targetSide, shipId);

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().HaveCount(1);

        var shipSunkEvent = shipSunkEvents.First();
        shipSunkEvent.GameId.Should().Be(game.Id);
        shipSunkEvent.AttackedSide.Should().Be(targetSide);
        shipSunkEvent.ShipId.Should().Be(shipId);
        shipSunkEvent.EventId.Should().NotBeEmpty();
        shipSunkEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        shipSunkEvent.EventType.Should().Be(typeof(ShipSunkEvent));
    }

    [Fact]
    public void Attack_WhenShipPartiallyHit_ShouldNotRaiseShipSunkEvent()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();

        // Act - Attack only one cell of the destroyer (not sinking it)
        game.Attack(BoardSide.Opponent, "A1"); // Hit first cell only

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().BeEmpty();
    }

    [Fact]
    public void Attack_WhenMultipleShipsSunk_ShouldRaiseMultipleShipSunkEvents()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        var shipIds = game.GetShips(BoardSide.Opponent).Take(2).ToList();

        // Act - Sink both ships
        _fixture.AttackShip(game, BoardSide.Opponent, shipIds[0]);
        _fixture.AttackShip(game, BoardSide.Opponent, shipIds[1]);

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().HaveCount(2);

        // All events should be for the same game and attacked side
        shipSunkEvents.Should().OnlyContain(e => e.GameId == game.Id);
        shipSunkEvents.Should().OnlyContain(e => e.AttackedSide == BoardSide.Opponent);
        shipSunkEvents.Should().Contain(e => e.ShipId == shipIds[0]);
        shipSunkEvents.Should().Contain(e => e.ShipId == shipIds[1]);
    }

    [Fact]
    public void Attack_WhenMissedAttack_ShouldNotRaiseShipSunkEvent()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();

        // Act - Attack empty cell
        game.Attack(BoardSide.Opponent, "B1");

        // Assert
        var shipSunkEvents = game.DomainEvents.OfType<ShipSunkEvent>().ToList();
        shipSunkEvents.Should().BeEmpty();
    }
}
