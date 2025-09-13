using System;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.PlayerAggregate;

public class PlayerTests
{
    private readonly PlayerId _playerId = new(Guid.NewGuid());
    private readonly string _validUsername = "TestPlayer";

    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreatePlayer()
    {
        var player = new Player(_playerId, _validUsername);

        player.Id.Should().Be(_playerId);
        player.Username.Should().Be(_validUsername);
        player.ActiveGameId.Should().BeNull();
        player.IsInActiveGame.Should().BeFalse();
        player.GameHistory.Should().BeEmpty();
        player.TotalGamesPlayed.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_WhenInvalidUsername_ShouldThrowArgumentException(string? invalidUsername)
    {
        var act = () => new Player(_playerId, invalidUsername!);

        const string exceptionMessage = "Username cannot be null or whitespace.*";
        act.Should().Throw<ArgumentException>().WithMessage(exceptionMessage).And.ParamName.Should().Be("username");
    }

    [Fact]
    public void JoinGame_WhenNotInActiveGame_ShouldJoinGame()
    {
        var player = new Player(_playerId, _validUsername);
        var gameId = new GameId(Guid.NewGuid());

        player.JoinGame(gameId);

        player.ActiveGameId.Should().Be(gameId);
        player.IsInActiveGame.Should().BeTrue();
        player.TotalGamesPlayed.Should().Be(1);

        // Verify domain event was raised
        player.DomainEvents.Should().HaveCount(1);
        var domainEvent = player.DomainEvents[0].Should().BeOfType<PlayerJoinedGameEvent>().Subject;
        domainEvent.PlayerId.Should().Be(_playerId);
        domainEvent.GameId.Should().Be(gameId);
        domainEvent.Username.Should().Be(_validUsername);
    }

    [Fact]
    public void JoinGame_WhenAlreadyInActiveGame_ShouldThrowInvalidOperationException()
    {
        var player = new Player(_playerId, _validUsername);
        var gameId1 = new GameId(Guid.NewGuid());
        var gameId2 = new GameId(Guid.NewGuid());

        player.JoinGame(gameId1);

        var act = () => player.JoinGame(gameId2);

        const string exceptionMessage = "Player is already in an active game.";
        act.Should().Throw<InvalidOperationException>().WithMessage(exceptionMessage);
    }

    [Fact]
    public void LeaveGame_WhenInActiveGame_ShouldLeaveGameAndAddToHistory()
    {
        var player = new Player(_playerId, _validUsername);
        var gameId = new GameId(Guid.NewGuid());
        player.JoinGame(gameId);
        player.ClearDomainEvents(); // Clear join event to focus on leave event

        player.LeaveGame();

        player.ActiveGameId.Should().BeNull();
        player.IsInActiveGame.Should().BeFalse();
        player.GameHistory.Should().ContainSingle().Which.Should().Be(gameId);
        player.TotalGamesPlayed.Should().Be(1);

        // Verify domain event was raised
        player.DomainEvents.Should().HaveCount(1);
        var domainEvent = player.DomainEvents[0].Should().BeOfType<PlayerLeftGameEvent>().Subject;
        domainEvent.PlayerId.Should().Be(_playerId);
        domainEvent.GameId.Should().Be(gameId);
        domainEvent.Username.Should().Be(_validUsername);
    }

    [Fact]
    public void LeaveGame_WhenNotInActiveGame_ShouldThrowInvalidOperationException()
    {
        var player = new Player(_playerId, _validUsername);

        var act = () => player.LeaveGame();

        const string exceptionMessage = "Player is not in an active game.";
        act.Should().Throw<InvalidOperationException>().WithMessage(exceptionMessage);
    }

    [Fact]
    public void TotalGamesPlayed_WhenMultipleGamesCompleted_ShouldReturnCorrectCount()
    {
        var player = new Player(_playerId, _validUsername);

        // Complete first game
        var gameId1 = new GameId(Guid.NewGuid());
        player.JoinGame(gameId1);
        player.LeaveGame();

        // Complete second game
        var gameId2 = new GameId(Guid.NewGuid());
        player.JoinGame(gameId2);
        player.LeaveGame();

        // Join third game (still active)
        var gameId3 = new GameId(Guid.NewGuid());
        player.JoinGame(gameId3);

        player.GameHistory.Should().HaveCount(2);
        player.GameHistory.Should().Contain(gameId1);
        player.GameHistory.Should().Contain(gameId2);
        player.ActiveGameId.Should().Be(gameId3);
        player.TotalGamesPlayed.Should().Be(3); // 2 completed + 1 active
    }

    [Fact]
    public void GameHistory_ShouldBeReadOnly()
    {
        var player = new Player(_playerId, _validUsername);
        var gameId = new GameId(Guid.NewGuid());
        player.JoinGame(gameId);
        player.LeaveGame();

        var gameHistory = player.GameHistory;

        // Should be read-only collection
        gameHistory.Should().BeAssignableTo<System.Collections.Generic.IReadOnlyList<GameId>>();
    }
}
