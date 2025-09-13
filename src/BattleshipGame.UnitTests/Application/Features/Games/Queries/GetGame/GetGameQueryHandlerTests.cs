using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Games.Queries.GetGame;

public class GetGameQueryHandlerTests
{
    private readonly IGameRepository _gameRepository;
    private readonly GetGameQueryHandler _handler;
    private readonly GameFixture _gameFixture = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetGameQueryHandlerTests()
    {
        _gameRepository = A.Fake<IGameRepository>();
        _handler = new GetGameQueryHandler(_gameRepository);
    }

    [Fact]
    public async Task Handle_WhenGameExists_ShouldReturnGameResult()
    {
        // Arrange
        var game = _gameFixture.CreateNewGame();
        var query = new GetGameQuery(game.Id);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).Returns(game);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(game.Id);
        result.PlayerId.Should().Be(game.PlayerId.Value);
        result.BoardSize.Should().Be(10);
        result.State.Should().Be(GameState.Started);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenGameDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        var query = new GetGameQuery(gameId);

        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, _cancellationToken)).Returns<Game?>(null);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        result.Should().BeNull();

        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, _cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(26)]
    public async Task Handle_WithDifferentBoardSizes_ShouldReturnCorrectBoardSize(int boardSize)
    {
        // Arrange
        var game = _gameFixture.CreateNewGame(boardSize: boardSize);
        var query = new GetGameQuery(game.Id);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).Returns(game);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.BoardSize.Should().Be(boardSize);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        var query = new GetGameQuery(gameId);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, cts.Token)).Throws(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_WithDifferentPlayerIds_ShouldReturnCorrectPlayerId()
    {
        // Arrange
        var playerId1 = new PlayerId(Guid.NewGuid());
        var playerId2 = new PlayerId(Guid.NewGuid());
        var game = _gameFixture.CreateNewGame(playerId1);
        var query = new GetGameQuery(game.Id);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).Returns(game);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId1.Value);
        result.PlayerId.Should().NotBe(playerId2.Value);
    }

    [Fact]
    public async Task Handle_ShouldMapAllGamePropertiesCorrectly()
    {
        // Arrange
        const int expectedBoardSize = 10;
        const GameState expectedState = GameState.BoardsAreReady;

        var playerId = new PlayerId(Guid.NewGuid());
        var game = _gameFixture.CreateReadyGame(playerId);
        var query = new GetGameQuery(game.Id);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).Returns(game);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(game.Id);
        result.PlayerId.Should().Be(playerId.Value);
        result.BoardSize.Should().Be(expectedBoardSize);
        result.State.Should().Be(expectedState);
    }
}
