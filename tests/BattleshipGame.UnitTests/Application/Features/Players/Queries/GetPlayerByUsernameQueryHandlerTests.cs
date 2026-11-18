using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;

namespace BattleshipGame.UnitTests.Application.Features.Players.Queries;

public class GetPlayerByUsernameQueryHandlerTests
{
    private readonly IPlayerRepository _playerRepository = A.Fake<IPlayerRepository>();
    private readonly IGameRepository _gameRepository = A.Fake<IGameRepository>();
    private readonly GetPlayerByUsernameHandler _handler;

    private readonly PlayerId _playerId = new(Guid.NewGuid());
    private readonly CancellationToken _ct = CancellationToken.None;

    public GetPlayerByUsernameQueryHandlerTests() =>
        _handler = new GetPlayerByUsernameHandler(_playerRepository, _gameRepository);

    [Fact]
    public async Task Handle_WhenPlayerExistsByUsername_ShouldReturnPlayerResult()
    {
        // Arrange
        const string username = "TestPlayer";
        ArrangePlayer(username);

        // Act
        var result = await _handler.Handle(new GetPlayerByUsernameQuery(username), _ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(_playerId);
        result.Username.Should().Be(username);
        result.ActiveGameId.Should().BeNull();
        result.TotalGamesPlayed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenPlayerHasGames_ShouldReturnActiveGameAndCountFromGames()
    {
        // Arrange
        const string username = "ActivePlayer";
        var activeGame = new GameFixture().CreateGameInStateStarted(_playerId);
        ArrangePlayer(username);
        A.CallTo(() => _gameRepository.GetActiveGameByPlayerIdAsync(_playerId, _ct))
            .Returns(activeGame);
        A.CallTo(() => _gameRepository.CountByPlayerIdAsync(_playerId, _ct)).Returns(4);

        // Act
        var result = await _handler.Handle(new GetPlayerByUsernameQuery(username), _ct);

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameId.Should().Be(activeGame.Id.Value);
        result.TotalGamesPlayed.Should().Be(4);
    }

    [Theory]
    [InlineData("NonExistentPlayer")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_WhenPlayerDoesNotExist_ShouldReturnNullWithoutReadingGames(
        string? username
    )
    {
        // Arrange
        A.CallTo(() => _playerRepository.GetByUsernameAsync(username!, _ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        var result = await _handler.Handle(new GetPlayerByUsernameQuery(username!), _ct);

        // Assert
        result.Should().BeNull();
        A.CallTo(() => _playerRepository.GetByUsernameAsync(username!, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(_gameRepository).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldLookUpTheExactUsernameRequested()
    {
        // Arrange
        A.CallTo(() => _playerRepository.GetByUsernameAsync(A<string>._, _ct))
            .Returns(Task.FromResult<Player?>(null));

        // Act
        await _handler.Handle(new GetPlayerByUsernameQuery("TestPlayer"), _ct);

        // Assert
        A.CallTo(() => _playerRepository.GetByUsernameAsync("TestPlayer", _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");
        A.CallTo(() => _playerRepository.GetByUsernameAsync("TestPlayer", _ct))
            .Throws(expectedException);

        // Act
        var act = () => _handler.Handle(new GetPlayerByUsernameQuery("TestPlayer"), _ct);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should()
            .BeSameAs(expectedException);
    }

    private void ArrangePlayer(string username) =>
        A.CallTo(() => _playerRepository.GetByUsernameAsync(username, _ct))
            .Returns(new Player(_playerId, username, "auth|subject"));
}
