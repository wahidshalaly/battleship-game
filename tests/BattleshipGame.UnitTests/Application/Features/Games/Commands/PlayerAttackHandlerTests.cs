using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.Exceptions;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class PlayerAttackHandlerTests
{
    private const string Ship1Location = "A1";

    private readonly PlayerAttackHandler _subject;
    private readonly IGameRepository _gameRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly GameFixture _gameFixture = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public PlayerAttackHandlerTests()
    {
        var logger = A.Fake<ILogger<PlayerAttackHandler>>();
        _gameRepository = A.Fake<IGameRepository>();
        _eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _subject = new PlayerAttackHandler(logger, _gameRepository, _eventDispatcher);
    }

    [Fact]
    public async Task Handle_WhenAttackHitsOccupiedCell_ShouldReturnHitResultAndDispatchEvents()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted(); // Must be started, not just ready
        var command = new PlayerAttackCommand(game.Id, Ship1Location);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(game.Id, _cancellationToken))
            .Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _subject.Handle(command, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.CellState.Should().Be(CellState.Hit);
        result.TargetCell.Should().Be(Ship1Location);
        result.GameState.Should().Be(GameState.Started);
        result.WinnerSide.Should().Be(BoardSide.None);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(game.Id, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _gameRepository.SaveAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenValidAttackMisses_ShouldReturnMissResult()
    {
        // Arrange
        const string cellCode = "F1"; // Empty cell
        var game = _gameFixture.CreateGameInStateStarted(); // Must be started, not just ready
        var command = new PlayerAttackCommand(game.Id, cellCode);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(game.Id, _cancellationToken))
            .Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _subject.Handle(command, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.CellState.Should().Be(CellState.Missed);
        result.TargetCell.Should().Be(cellCode);
        result.GameState.Should().Be(GameState.Started);
        result.WinnerSide.Should().Be(BoardSide.None);

        A.CallTo(() => _gameRepository.SaveAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenGameNotStarted_ShouldThrowException()
    {
        // Arrange - Game is Ready but not Started
        var game = _gameFixture.CreateGameInStateReady();
        const string cellCode = "A1";
        var command = new PlayerAttackCommand(game.Id, cellCode);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(game.Id, _cancellationToken))
            .Returns(game);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GameNotStartedException>(() =>
            _subject.Handle(command, _cancellationToken)
        );
        exception.Message.Should().Contain("`" + game.Id.Value + "`");
        exception.Message.Should().Contain("`" + game.State + "`");

        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, _cancellationToken))
            .MustNotHaveHappened();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(A<Game>._, _cancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldDispatchAllEvents()
    {
        // Arrange
        const string cellCode = "A1";
        var game = _gameFixture.CreateGameInStateStarted(); // Must be started, not just ready
        var command = new PlayerAttackCommand(game.Id, cellCode);

        A.CallTo(() => _gameRepository.GetByIdOrThrowAsync(game.Id, _cancellationToken))
            .Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _subject.Handle(command, _cancellationToken);

        // Assert
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
    }
}
