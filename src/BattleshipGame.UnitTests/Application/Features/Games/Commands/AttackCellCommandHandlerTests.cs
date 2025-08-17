using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class AttackCellCommandHandlerTests
{
    private const string Ship1Location = "A1";

    private readonly IGameRepository _gameRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly AttackCellCommandHandler _handler;
    private readonly GameFixture _gameFixture = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    public AttackCellCommandHandlerTests()
    {
        var logger = A.Fake<ILogger<AttackCellCommandHandler>>();
        _gameRepository = A.Fake<IGameRepository>();
        _eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _handler = new AttackCellCommandHandler(logger, _gameRepository, _eventDispatcher);
    }

    [Fact]
    public async Task Handle_WhenAttackHitsOccupiedCell_ShouldReturnHitResultAndDispatchEvents()
    {
        // Arrange
        const BoardSide boardSide = BoardSide.Opp;
        var game = _gameFixture.CreateReadyGame();
        var command = new AttackCellCommand(game.Id, boardSide, Ship1Location);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _ct)).Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, _ct);

        // Assert
        result.Should().NotBeNull();
        result.IsHit.Should().BeTrue();
        result.IsGameOver.Should().BeFalse();

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _gameRepository.SaveAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenValidAttackMisses_ShouldReturnMissResult()
    {
        // Arrange
        const string cellCode = "F1"; // Empty cell
        const BoardSide boardSide = BoardSide.Opp;
        var game = _gameFixture.CreateReadyGame(); // Ship at A1, attacking B1
        var command = new AttackCellCommand(game.Id, boardSide, cellCode);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _ct)).Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, _ct);

        // Assert
        result.Should().NotBeNull();
        result.IsHit.Should().BeFalse();
        result.IsGameOver.Should().BeFalse();

        A.CallTo(() => _gameRepository.SaveAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenAttackResultsInGameWin_ShouldReturnGameOverResult()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        const BoardSide boardSide = BoardSide.Opp;
        var game = _gameFixture.CreateReadyGame(playerId);
        var shipIds = game.GetShips(boardSide);
        var cells = shipIds.SelectMany(shipId => game.GetShipPosition(boardSide, shipId)).ToList();
        var lastCell = cells.Last();
        var restOfCellsToAttach = cells.Except([lastCell]).ToList();
        restOfCellsToAttach.ForEach(cell => game.Attack(boardSide, cell));

        var command = new AttackCellCommand(game.Id, boardSide, lastCell);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _ct)).Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, _ct);

        // Assert
        result.Should().NotBeNull();
        result.IsHit.Should().BeTrue();
        result.IsGameOver.Should().BeTrue();

        A.CallTo(() => _gameRepository.SaveAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(game, _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string cellCode = "A1";
        const BoardSide boardSide = BoardSide.Opp;
        var gameId = new GameId(Guid.NewGuid());
        var command = new AttackCellCommand(gameId, boardSide, cellCode);

        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, _ct)).Returns<Game?>(null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, _ct));

        exception.Message.Should().Contain($"Game {gameId} not found");

        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, _ct))
            .MustNotHaveHappened();
        A.CallTo(() => _eventDispatcher.DispatchEventsAsync(A<Game>._, _ct))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldClearDomainEventsAfterDispatching()
    {
        // Arrange
        const string cellCode = "A1";
        const BoardSide boardSide = BoardSide.Opp;
        var game = _gameFixture.CreateReadyGame();
        var command = new AttackCellCommand(game.Id, boardSide, cellCode);

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _ct)).Returns(game);
        A.CallTo(() => _gameRepository.SaveAsync(game, _ct)).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, _ct);

        // Assert
        game.DomainEvents.Should().BeEmpty();
    }
}