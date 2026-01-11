using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate.Events;

public class GameOverEventTests
{
    private readonly GameFixture _fixture = new();

    [Theory]
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
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
    [InlineData(BoardSide.Player)]
    [InlineData(BoardSide.Opponent)]
    public void Attack_WhenGameOver_ShouldRaiseGameOverEvent(BoardSide winnerSide)
    {
        // Arrange & Act
        var game = _fixture.GetFinishedGame(null, winnerSide);
        var defeatedSide = winnerSide.OppositeSide();

        // Assert
        game.State.Should().Be(GameState.GameOver);
        game.IsGameOver(defeatedSide).Should().BeTrue();
        game.WinnerSide.Should().Be(winnerSide);

        var gameOverEvents = game.DomainEvents.OfType<GameOverEvent>().ToList();
        gameOverEvents.Should().HaveCount(1);

        var gameOverEvent = gameOverEvents.First();
        gameOverEvent.GameId.Should().Be(game.Id);
        gameOverEvent.WinnerSide.Should().Be(winnerSide);
    }
}
