using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.Exceptions;
using BattleshipGame.UnitTests.Domain.DomainModel;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class StartGameplayHandlerTests
{
    readonly StartGameplayHandler _subject;
    private readonly ILogger<StartGameplayHandler> _logger;
    private readonly IGameRepository _gameRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly GameFixture _gameFixture = new();

    public StartGameplayHandlerTests()
    {
        _logger = A.Fake<ILogger<StartGameplayHandler>>();
        _gameRepository = A.Fake<IGameRepository>();
        _eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _subject = new StartGameplayHandler(_logger, _gameRepository, _eventDispatcher);
    }

    [Fact]
    public async Task Handle_WhenGameIsReady_ShouldStartGameplay()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateReady();
        var gameId = new GameId(game.Id.Value);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(gameId, A<CancellationToken>._))
            .Returns(game);

        var command = new StartGameplayCommand(gameId);

        // Act
        await _subject.Handle(command, CancellationToken.None);

        // Assert
        game.State.Should().Be(GameState.Started);
        A.CallTo(() => _gameRepository.SaveAsync(game, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenGameNotReady_ShouldThrowGameNotReadyException()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateNew();
        var gameId = new GameId(game.Id.Value);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(gameId, A<CancellationToken>._))
            .Returns(game);

        var command = new StartGameplayCommand(gameId);

        // Act
        var act = () => _subject.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<GameNotReadyException>();
    }

    [Fact]
    public async Task Handle_WhenGameAlreadyStarted_ShouldThrowGameNotReadyException()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var gameId = new GameId(game.Id.Value);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(gameId, A<CancellationToken>._))
            .Returns(game);

        var command = new StartGameplayCommand(gameId);

        // Act
        var act = () => _subject.Handle(command, CancellationToken.None);
        // Assert
        await act.Should().ThrowAsync<GameNotReadyException>();
    }
}
