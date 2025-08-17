using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class CreateGameCommandHandlerTests
{
    private readonly IGameRepository _gameRepository;
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTests()
    {
        var logger = A.Fake<ILogger<CreateGameCommandHandler>>();
        _gameRepository = A.Fake<IGameRepository>();
        var eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _handler = new CreateGameCommandHandler(logger, _gameRepository, eventDispatcher);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateGameAndReturnResult()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        const int boardSize = 12;
        var command = new CreateGameCommand(playerId, boardSize);
        var ct = CancellationToken.None;

        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, ct))
            .Returns(Task.FromResult(new GameId(Guid.NewGuid())));

        // Act
        var result = await _handler.Handle(command, ct);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().NotBe(Guid.Empty);

        A.CallTo(() => _gameRepository.SaveAsync(
            A<Game>.That.Matches(g =>
                g.PlayerId == playerId &&
                g.BoardSize == boardSize &&
                g.State == GameState.Started), ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNullBoardSize_ShouldUseDefaultBoardSize()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var command = new CreateGameCommand(playerId, null);
        var ct = CancellationToken.None;

        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, ct))
            .Returns(Task.FromResult(new GameId(Guid.NewGuid())));

        // Act
        var result = await _handler.Handle(command, ct);

        // Assert
        result.GameId.Should().NotBe(Guid.Empty);

        A.CallTo(() => _gameRepository.SaveAsync(
            A<Game>.That.Matches(g => g.BoardSize == DefaultBoardSize), ct))
            .MustHaveHappenedOnceExactly();
    }
}
