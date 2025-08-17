using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Players.Queries;

public class GetPlayerByUsernameQueryHandlerTests
{
    private readonly IPlayerRepository _playerRepository;
    private readonly GetPlayerByUsernameQueryHandler _handler;

    public GetPlayerByUsernameQueryHandlerTests()
    {
        _playerRepository = A.Fake<IPlayerRepository>();
        _handler = new GetPlayerByUsernameQueryHandler(_playerRepository);
    }

    [Fact]
    public async Task Handle_WhenPlayerExistsByUsername_ShouldReturnPlayerResult()
    {
        // Arrange
        const string username = "TestPlayer";
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, null, 0);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId);
        result.Username.Should().Be(username);
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(0);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenPlayerDoesNotExistByUsername_ShouldReturnNull()
    {
        // Arrange
        const string username = "NonExistentPlayer";
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        const string username = "TestPlayer";
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database connection failed");

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Throws(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, ct));

        actualException.Should().BeSameAs(expectedException);
    }

    [Fact]
    public async Task Handle_WhenPlayerHasActiveGame_ShouldReturnActiveGameId()
    {
        // Arrange
        const string username = "PlayerWithGame";
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, activeGameId, 0);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().Be(activeGameId.Value);
        result.TotalGamesPlayed.Should().Be(1); // Active game counts as 1
    }

    [Fact]
    public async Task Handle_WhenPlayerHasNoActiveGameButHasHistory_ShouldReturnCorrectTotalGames()
    {
        // Arrange
        const string username = "ExperiencedPlayer";
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, null, 3);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenPlayerHasActiveGameAndHistory_ShouldReturnCorrectTotalGames()
    {
        // Arrange
        const string username = "ActivePlayer";
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, activeGameId, 5);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().Be(activeGameId.Value);
        result.TotalGamesPlayed.Should().Be(6); // 5 from history + 1 active
    }

    [Theory]
    [InlineData("TestUser")]
    [InlineData("Player123")]
    [InlineData("AnotherPlayer")]
    [InlineData("user_with_underscores")]
    [InlineData("UPPERCASE")]
    [InlineData("MixedCasePlayer")]
    public async Task Handle_WithDifferentUsernames_ShouldFindCorrectPlayer(string username)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, null, 0);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.PlayerId.Should().Be(playerId);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        const string username = "TestPlayer";
        var query = new GetPlayerByUsernameQuery(username);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, cts.Token))
            .Throws(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_ShouldMapAllPlayerPropertiesCorrectly()
    {
        // Arrange
        const string expectedUsername = "CompleteTestPlayer";
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(expectedUsername);
        var ct = CancellationToken.None;
        const int expectedHistoryCount = 7;

        var player = CreatePlayer(playerId, expectedUsername, activeGameId, expectedHistoryCount);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(expectedUsername, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId);
        result.PlayerId.Value.Should().Be(playerId.Value);
        result.Username.Should().Be(expectedUsername);
        result.ActiveGameId.Should().Be(activeGameId.Value);
        result.TotalGamesPlayed.Should().Be(expectedHistoryCount + 1); // History + active game
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Handle_WithDifferentGameHistoryCounts_ShouldReturnCorrectTotalGames(int historyCount)
    {
        // Arrange
        const string username = "TestPlayer";
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, null, historyCount);

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.TotalGamesPlayed.Should().Be(historyCount);
    }

    [Fact]
    public async Task Handle_WithNullUsername_ShouldCallRepositoryWithNull()
    {
        // Arrange
        string? username = null;
        var query = new GetPlayerByUsernameQuery(username!);
        var ct = CancellationToken.None;

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username!, ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username!, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithEmptyUsername_ShouldCallRepositoryWithEmptyString()
    {
        // Arrange
        const string username = "";
        var query = new GetPlayerByUsernameQuery(username);
        var ct = CancellationToken.None;

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();

        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ShouldUseExactUsernameMatch()
    {
        // Arrange
        const string requestedUsername = "TestPlayer";
        const string differentUsername = "testPlayer"; // Different case
        var query = new GetPlayerByUsernameQuery(requestedUsername);
        var ct = CancellationToken.None;

        // Repository should be called with exact username
        A.CallTo(() => _playerRepository.GetByUsernameAsync(requestedUsername, ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();

        // Verify it was called with the exact username, not a different one
        A.CallTo(() => _playerRepository.GetByUsernameAsync(requestedUsername, ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _playerRepository.GetByUsernameAsync(differentUsername, ct))
            .MustNotHaveHappened();
    }

    private static Player CreatePlayer(PlayerId playerId, string username, GameId? activeGameId, int gameHistoryCount)
    {
        var player = new Player(playerId, username, activeGameId);

        // Use reflection to add games to history to simulate TotalGamesPlayed
        var gameHistoryField = typeof(Player).GetField("_gameHistory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (gameHistoryField != null)
        {
            var gameHistory = (System.Collections.Generic.List<GameId>)gameHistoryField.GetValue(player)!;
            for (int i = 0; i < gameHistoryCount; i++)
            {
                gameHistory.Add(new GameId(Guid.NewGuid()));
            }
        }

        return player;
    }
}