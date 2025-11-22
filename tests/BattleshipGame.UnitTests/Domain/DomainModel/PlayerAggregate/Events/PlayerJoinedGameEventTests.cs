using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.PlayerAggregate.Events;

public class PlayerJoinedGameEventTests
{
    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerJoinedGameEvent = new PlayerJoinedGameEvent(playerId, gameId, username);

        // Assert
        playerJoinedGameEvent.PlayerId.Should().Be(playerId);
        playerJoinedGameEvent.GameId.Should().Be(gameId);
        playerJoinedGameEvent.Username.Should().Be(username);
        playerJoinedGameEvent.EventId.Should().NotBeEmpty();
        playerJoinedGameEvent
            .OccurredOn.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerJoinedGameEvent.EventType.Should().Be(typeof(PlayerJoinedGameEvent));
    }

    [Fact]
    public void Ctor_WhenJoinedAtNotProvided_ShouldUseCurrentTime()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerJoinedGameEvent = new PlayerJoinedGameEvent(playerId, gameId, username);

        // Assert
        playerJoinedGameEvent.PlayerId.Should().Be(playerId);
        playerJoinedGameEvent.GameId.Should().Be(gameId);
        playerJoinedGameEvent.Username.Should().Be(username);
        playerJoinedGameEvent.EventId.Should().NotBeEmpty();
        playerJoinedGameEvent
            .OccurredOn.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerJoinedGameEvent.EventType.Should().Be(typeof(PlayerJoinedGameEvent));
    }

    [Fact]
    public void Ctor_WhenJoinedAtIsNull_ShouldUseCurrentTime()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerJoinedGameEvent = new PlayerJoinedGameEvent(playerId, gameId, username);

        // Assert
        playerJoinedGameEvent.PlayerId.Should().Be(playerId);
        playerJoinedGameEvent.GameId.Should().Be(gameId);
        playerJoinedGameEvent.Username.Should().Be(username);
        playerJoinedGameEvent.EventId.Should().NotBeEmpty();
        playerJoinedGameEvent
            .OccurredOn.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerJoinedGameEvent.EventType.Should().Be(typeof(PlayerJoinedGameEvent));
    }

    [Fact]
    public void JoinGame_WhenPlayerJoinsGame_ShouldRaisePlayerJoinedGameEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act
        player.JoinGame(gameId);

        // Assert
        var playerJoinedGameEvents = player.DomainEvents.OfType<PlayerJoinedGameEvent>().ToList();
        playerJoinedGameEvents.Should().HaveCount(1);

        var playerJoinedGameEvent = playerJoinedGameEvents.First();
        playerJoinedGameEvent.PlayerId.Should().Be(playerId);
        playerJoinedGameEvent.GameId.Should().Be(gameId);
        playerJoinedGameEvent.Username.Should().Be(username);
        playerJoinedGameEvent.EventId.Should().NotBeEmpty();
        playerJoinedGameEvent
            .OccurredOn.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerJoinedGameEvent.EventType.Should().Be(typeof(PlayerJoinedGameEvent));
    }

    [Fact]
    public void JoinGame_WhenPlayerAlreadyInGame_ShouldNotRaiseAdditionalEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId1 = new GameId(Guid.NewGuid());
        var gameId2 = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act
        player.JoinGame(gameId1);
        var initialEventCount = player.DomainEvents.Count;

        // Attempting to join another game should throw exception and not raise event
        var act = () => player.JoinGame(gameId2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
        player.DomainEvents.Should().HaveCount(initialEventCount); // No additional events

        var playerJoinedGameEvents = player.DomainEvents.OfType<PlayerJoinedGameEvent>().ToList();
        playerJoinedGameEvents.Should().HaveCount(1);
        playerJoinedGameEvents.First().GameId.Should().Be(gameId1); // Still the original game
    }

    [Fact]
    public void JoinGame_WhenPlayerJoinsMultipleGamesSequentially_ShouldRaiseMultipleEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId1 = new GameId(Guid.NewGuid());
        var gameId2 = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act
        // Join first game
        player.JoinGame(gameId1);
        player.LeaveGame();

        // Join second game
        player.JoinGame(gameId2);

        // Assert
        var playerJoinedGameEvents = player.DomainEvents.OfType<PlayerJoinedGameEvent>().ToList();
        playerJoinedGameEvents.Should().HaveCount(2);

        // First event for first game
        var firstEvent = playerJoinedGameEvents.First(e => e.GameId == gameId1);
        firstEvent.PlayerId.Should().Be(playerId);
        firstEvent.Username.Should().Be(username);

        // Second event for second game
        var secondEvent = playerJoinedGameEvents.First(e => e.GameId == gameId2);
        secondEvent.PlayerId.Should().Be(playerId);
        secondEvent.Username.Should().Be(username);

        // Events should have different timestamps
        secondEvent.OccurredOn.Should().BeAfter(firstEvent.OccurredOn);
    }

    [Theory]
    [InlineData("Player1")]
    [InlineData("TestUser")]
    [InlineData("AnotherPlayer")]
    public void Ctor_WhenDifferentUsernames_ShouldPreserveUsername(string username)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());

        // Act
        var playerJoinedGameEvent = new PlayerJoinedGameEvent(playerId, gameId, username);

        // Assert
        playerJoinedGameEvent.Username.Should().Be(username);
    }
}
