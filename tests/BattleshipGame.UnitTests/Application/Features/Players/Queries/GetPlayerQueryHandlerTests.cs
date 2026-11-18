using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;

namespace BattleshipGame.UnitTests.Application.Features.Players.Queries;

public class GetPlayerQueryHandlerTests
{
    private readonly IPlayerRepository _playerRepository = A.Fake<IPlayerRepository>();
    private readonly IGameRepository _gameRepository = A.Fake<IGameRepository>();
    private readonly GetPlayerByIdHandler _handler;

    private readonly PlayerId _playerId = new(Guid.NewGuid());
    private readonly CancellationToken _ct = CancellationToken.None;

    public GetPlayerQueryHandlerTests() =>
        _handler = new GetPlayerByIdHandler(_playerRepository, _gameRepository);

    [Fact]
    public async Task Handle_WhenPlayerHasNoGames_ShouldReturnPlayerWithoutActiveGame()
    {
        // Arrange
        ArrangePlayer("TestPlayer");
        ArrangeGames(activeGame: null, totalGames: 0);

        // Act
        var result = await _handler.Handle(new GetPlayerByIdQuery(_playerId), _ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(_playerId);
        result.Username.Should().Be("TestPlayer");
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenPlayerHasActiveGame_ShouldReturnItsId()
    {
        // Arrange
        var activeGame = new GameFixture().CreateGameInStateStarted(_playerId);
        ArrangePlayer("ActivePlayer");
        ArrangeGames(activeGame, totalGames: 3);

        // Act
        var result = await _handler.Handle(new GetPlayerByIdQuery(_playerId), _ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().Be(activeGame.Id.Value);
        result.TotalGamesPlayed.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenPlayerHasOnlyFinishedGames_ShouldReturnNoActiveGameAndTheirCount()
    {
        // Arrange
        ArrangePlayer("ExperiencedPlayer");
        ArrangeGames(activeGame: null, totalGames: 5);

        // Act
        var result = await _handler.Handle(new GetPlayerByIdQuery(_playerId), _ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenPlayerDoesNotExist_ShouldReturnNullWithoutReadingGames()
    {
        // Arrange
        A.CallTo(() => _playerRepository.GetByIdAsync(_playerId, _ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(new GetPlayerByIdQuery(_playerId), _ct);

        // Assert
        result.Should().BeNull();
        A.CallTo(_gameRepository).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");
        A.CallTo(() => _playerRepository.GetByIdAsync(_playerId, _ct)).Throws(expectedException);

        // Act
        var act = () => _handler.Handle(new GetPlayerByIdQuery(_playerId), _ct);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should()
            .BeSameAs(expectedException);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepositories()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        A.CallTo(() => _playerRepository.GetByIdAsync(_playerId, cts.Token))
            .Returns(new Player(_playerId, "TestPlayer", "auth|subject"));

        // Act
        await _handler.Handle(new GetPlayerByIdQuery(_playerId), cts.Token);

        // Assert
        A.CallTo(() => _gameRepository.GetActiveGameByPlayerIdAsync(_playerId, cts.Token))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _gameRepository.CountByPlayerIdAsync(_playerId, cts.Token))
            .MustHaveHappenedOnceExactly();
    }

    private void ArrangePlayer(string username) =>
        A.CallTo(() => _playerRepository.GetByIdAsync(_playerId, _ct))
            .Returns(new Player(_playerId, username, "auth|subject"));

    private void ArrangeGames(Game? activeGame, int totalGames)
    {
        A.CallTo(() => _gameRepository.GetActiveGameByPlayerIdAsync(_playerId, _ct))
            .Returns(activeGame);
        A.CallTo(() => _gameRepository.CountByPlayerIdAsync(_playerId, _ct)).Returns(totalGames);
    }
}
