using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class GameOverEventTests
{
    [Theory]
    [InlineData(BoardSide.Own)]
    [InlineData(BoardSide.Opp)]
    public void Ctor_WhenValidParameters_ShouldCreateEvent(BoardSide winnerSide)
    {
        var gameId = new GameId(Guid.NewGuid());

        var gameOverEvent = new GameOverEvent(gameId, winnerSide);

        gameOverEvent.GameId.Should().Be(gameId);
        gameOverEvent.WinnerSide.Should().Be(winnerSide);
        gameOverEvent.EventId.Should().NotBeEmpty();
        gameOverEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        gameOverEvent.EventType.Should().Be(typeof(GameOverEvent));
    }

    [Theory]
    [InlineData(BoardSide.Own, BoardSide.Opp)]
    [InlineData(BoardSide.Opp, BoardSide.Own)]
    public void Attack_WhenGameOver_ShouldRaiseGameOverEvent(BoardSide attackedSide, BoardSide attackerSide)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);

        // Set up minimal ships for quick game over - single destroyer (2 cells)
        game.AddShip(attackerSide, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");
        game.AddShip(attackerSide, ShipKind.Cruiser, ShipOrientation.Vertical, "C1");
        game.AddShip(attackerSide, ShipKind.Submarine, ShipOrientation.Vertical, "E1");
        game.AddShip(attackerSide, ShipKind.Battleship, ShipOrientation.Vertical, "G1");
        game.AddShip(attackerSide, ShipKind.Carrier, ShipOrientation.Vertical, "I1");

        game.AddShip(attackedSide, ShipKind.Destroyer, ShipOrientation.Vertical, "A1");

        // Act - Attack all cells of the destroyer to sink it and end the game
        game.Attack(attackedSide, "A1"); // Hit first cell
        game.Attack(attackedSide, "A2"); // Hit second cell and sink the ship

        // Assert
        game.State.Should().Be(GameState.GameOver);
        game.IsGameOver(attackedSide).Should().BeTrue();

        var gameOverEvents = game.DomainEvents.OfType<GameOverEvent>().ToList();
        gameOverEvents.Should().HaveCount(1);

        var gameOverEvent = gameOverEvents.First();
        gameOverEvent.GameId.Should().Be(game.Id);
        gameOverEvent.WinnerSide.Should().Be(attackedSide);
    }
}
