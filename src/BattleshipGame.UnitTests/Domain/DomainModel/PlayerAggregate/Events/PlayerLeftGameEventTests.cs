using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.PlayerAggregate.Events;

public class PlayerLeftGameEventTests
{
    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerLeftGameEvent = new PlayerLeftGameEvent(playerId, gameId, username);

        // Assert
        playerLeftGameEvent.PlayerId.Should().Be(playerId);
        playerLeftGameEvent.GameId.Should().Be(gameId);
        playerLeftGameEvent.Username.Should().Be(username);
        playerLeftGameEvent.EventId.Should().NotBeEmpty();
        playerLeftGameEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerLeftGameEvent.EventType.Should().Be(typeof(PlayerLeftGameEvent));
    }

    [Fact]
    public void Ctor_WhenLeftAtNotProvided_ShouldUseCurrentTime()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerLeftGameEvent = new PlayerLeftGameEvent(playerId, gameId, username);

        // Assert
        playerLeftGameEvent.PlayerId.Should().Be(playerId);
        playerLeftGameEvent.GameId.Should().Be(gameId);
        playerLeftGameEvent.Username.Should().Be(username);
        playerLeftGameEvent.EventId.Should().NotBeEmpty();
        playerLeftGameEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerLeftGameEvent.EventType.Should().Be(typeof(PlayerLeftGameEvent));
    }

    [Fact]
    public void Ctor_WhenLeftAtIsNull_ShouldUseCurrentTime()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";

        // Act
        var playerLeftGameEvent = new PlayerLeftGameEvent(playerId, gameId, username);

        // Assert
        playerLeftGameEvent.PlayerId.Should().Be(playerId);
        playerLeftGameEvent.GameId.Should().Be(gameId);
        playerLeftGameEvent.Username.Should().Be(username);
        playerLeftGameEvent.EventId.Should().NotBeEmpty();
        playerLeftGameEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerLeftGameEvent.EventType.Should().Be(typeof(PlayerLeftGameEvent));
    }

    [Fact]
    public void LeaveGame_WhenPlayerLeavesGame_ShouldRaisePlayerLeftGameEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Join game first
        player.JoinGame(gameId);
        player.ClearDomainEvents(); // Clear join event to focus on leave event

        // Act
        player.LeaveGame();

        // Assert
        var playerLeftGameEvents = player.DomainEvents.OfType<PlayerLeftGameEvent>().ToList();
        playerLeftGameEvents.Should().HaveCount(1);

        var playerLeftGameEvent = playerLeftGameEvents.First();
        playerLeftGameEvent.PlayerId.Should().Be(playerId);
        playerLeftGameEvent.GameId.Should().Be(gameId);
        playerLeftGameEvent.Username.Should().Be(username);
        playerLeftGameEvent.EventId.Should().NotBeEmpty();
        playerLeftGameEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        playerLeftGameEvent.EventType.Should().Be(typeof(PlayerLeftGameEvent));
    }

    [Fact]
    public void LeaveGame_WhenPlayerNotInGame_ShouldNotRaiseEvent()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act & Assert
        var act = () => player.LeaveGame();

        act.Should().Throw<InvalidOperationException>();

        var playerLeftGameEvents = player.DomainEvents.OfType<PlayerLeftGameEvent>().ToList();
        playerLeftGameEvents.Should().BeEmpty();
    }

    [Fact]
    public void LeaveGame_WhenPlayerLeavesMultipleGamesSequentially_ShouldRaiseMultipleEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId1 = new GameId(Guid.NewGuid());
        var gameId2 = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act
        // Join and leave first game
        player.JoinGame(gameId1);
        player.LeaveGame();

        // Join and leave second game
        player.JoinGame(gameId2);
        player.LeaveGame();

        // Assert
        var playerLeftGameEvents = player.DomainEvents.OfType<PlayerLeftGameEvent>().ToList();
        playerLeftGameEvents.Should().HaveCount(2);

        // First event for first game
        var firstEvent = playerLeftGameEvents.First(e => e.GameId == gameId1);
        firstEvent.PlayerId.Should().Be(playerId);
        firstEvent.Username.Should().Be(username);

        // Second event for second game
        var secondEvent = playerLeftGameEvents.First(e => e.GameId == gameId2);
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
        var playerLeftGameEvent = new PlayerLeftGameEvent(playerId, gameId, username);

        // Assert
        playerLeftGameEvent.Username.Should().Be(username);
    }

    [Fact]
    public void JoinAndLeaveGame_WhenPlayerCompletesGameCycle_ShouldRaiseBothEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var gameId = new GameId(Guid.NewGuid());
        const string username = "TestPlayer";
        var player = new Player(playerId, username);

        // Act
        player.JoinGame(gameId);
        player.LeaveGame();

        // Assert
        var joinEvents = player.DomainEvents.OfType<PlayerJoinedGameEvent>().ToList();
        var leaveEvents = player.DomainEvents.OfType<PlayerLeftGameEvent>().ToList();

        joinEvents.Should().HaveCount(1);
        leaveEvents.Should().HaveCount(1);

        var joinEvent = joinEvents.First();
        var leaveEvent = leaveEvents.First();

        // Both events should reference the same game and player
        joinEvent.GameId.Should().Be(gameId);
        leaveEvent.GameId.Should().Be(gameId);
        joinEvent.PlayerId.Should().Be(playerId);
        leaveEvent.PlayerId.Should().Be(playerId);
        joinEvent.Username.Should().Be(username);
        leaveEvent.Username.Should().Be(username);

        // Leave time should be after join time
        leaveEvent.OccurredOn.Should().BeAfter(joinEvent.OccurredOn);
    }
}