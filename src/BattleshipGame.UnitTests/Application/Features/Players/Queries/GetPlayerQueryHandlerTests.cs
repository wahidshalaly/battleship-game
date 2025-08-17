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

public class GetPlayerQueryHandlerTests
{
    private readonly IPlayerRepository _playerRepository;
    private readonly GetPlayerQueryHandler _handler;

    public GetPlayerQueryHandlerTests()
    {
        _playerRepository = A.Fake<IPlayerRepository>();
        _handler = new GetPlayerQueryHandler(_playerRepository);
    }

    [Fact]
    public async Task Handle_WhenPlayerExists_ShouldReturnPlayerResult()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, "TestPlayer", null, 0);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId);
        result.Username.Should().Be("TestPlayer");
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(0);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenPlayerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database connection failed");

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
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
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, "PlayerWithGame", activeGameId, 0);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
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
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, "ExperiencedPlayer", null, 3);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
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
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, "ActivePlayer", activeGameId, 5);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
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
    public async Task Handle_WithDifferentUsernames_ShouldReturnCorrectUsername(string username)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, username, null, 0);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, cts.Token))
            .Throws(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_ShouldMapAllPlayerPropertiesCorrectly()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var activeGameId = new GameId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;
        const string expectedUsername = "CompleteTestPlayer";
        const int expectedHistoryCount = 7;

        var player = CreatePlayer(playerId, expectedUsername, activeGameId, expectedHistoryCount);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
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
        var playerId = new PlayerId(Guid.NewGuid());
        var query = new GetPlayerQuery(playerId);
        var ct = CancellationToken.None;

        var player = CreatePlayer(playerId, "TestPlayer", null, historyCount);

        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct))
            .Returns(player);

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.TotalGamesPlayed.Should().Be(historyCount);
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