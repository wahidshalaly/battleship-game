using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class StartNewGameCommandHandlerTests
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly StartNewGameHandler _handler;

    public StartNewGameCommandHandlerTests()
    {
        var logger = A.Fake<ILogger<StartNewGameHandler>>();
        _gameRepository = A.Fake<IGameRepository>(x => x.Strict());
        _playerRepository = A.Fake<IPlayerRepository>(x => x.Strict());
        _eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _handler = new StartNewGameHandler(
            logger,
            _gameRepository,
            _playerRepository,
            _eventDispatcher
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldStartNewGameAndReturnResult()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        const int boardSize = 12;
        var command = new StartNewGameCommand(playerId, boardSize);
        var ct = CancellationToken.None;

        var player = new Player(playerId, "TestPlayer");
        A.CallTo(() => _playerRepository.SaveAsync(A<Player>._, ct))
            .Invokes((Player p, CancellationToken _) => player = p)
            .Returns(playerId);
        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct)).Returns(player);

        Game game = null!;
        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, ct))
            .Invokes((Game g, CancellationToken _) => game = g);

        // Act
        var result = await _handler.Handle(command, ct);

        // Assert
        result.Should().NotBe(Guid.Empty);

        A.CallTo(() =>
                _gameRepository.SaveAsync(
                    A<Game>.That.Matches(g =>
                        g.PlayerId == playerId
                        && g.BoardSize == boardSize
                        && g.State == GameState.New
                    ),
                    ct
                )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenNullBoardSize_ShouldUseDefaultBoardSize()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var command = new StartNewGameCommand(playerId);
        var ct = CancellationToken.None;

        var player = new Player(playerId, "TestPlayer");
        A.CallTo(() => _playerRepository.SaveAsync(A<Player>._, ct))
            .Invokes((Player p, CancellationToken _) => player = p)
            .Returns(playerId);
        A.CallTo(() => _playerRepository.GetByIdAsync(playerId, ct)).Returns(player);
        A.CallTo(() => _gameRepository.SaveAsync(A<Game>._, ct))
            .Returns(Task.FromResult(new GameId(Guid.NewGuid())));

        // Act
        var result = await _handler.Handle(command, ct);

        // Assert
        result.Should().NotBe(Guid.Empty);

        A.CallTo(() =>
                _gameRepository.SaveAsync(
                    A<Game>.That.Matches(g => g.BoardSize == DefaultBoardSize),
                    ct
                )
            )
            .MustHaveHappenedOnceExactly();
    }
}
