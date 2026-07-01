using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;

namespace BattleshipGame.UnitTests.Application.Services;

public class GameAccessGuardTests
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly IPlayerService _playerService = A.Fake<IPlayerService>();
    private readonly IGameRepository _gameRepository = A.Fake<IGameRepository>();
    private readonly GameAccessGuard _guard;

    public GameAccessGuardTests()
    {
        _guard = new GameAccessGuard(_playerService, _gameRepository);
    }

    private static Player OwnerWithId(out PlayerId id)
    {
        id = new PlayerId(Guid.NewGuid());
        return new Player(id, "owner", "sub-owner");
    }

    [Fact]
    public async Task EnsureOwnerAsync_WhenCallerOwnsGame_DoesNotThrow()
    {
        // Arrange
        var owner = OwnerWithId(out var ownerId);
        var game = new Game(ownerId);
        A.CallTo(() => _playerService.GetCurrentRequiredAsync(_ct)).Returns(owner);
        A.CallTo(() => _gameRepository.GetByIdAsync(A<GameId>._, _ct)).Returns(game);

        // Act
        var act = () => _guard.EnsureOwnerAsync(game.Id, _ct);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureOwnerAsync_WhenCallerIsNotOwner_ThrowsForbidden()
    {
        // Arrange
        var owner = OwnerWithId(out _);
        var game = new Game(new PlayerId(Guid.NewGuid())); // owned by someone else
        A.CallTo(() => _playerService.GetCurrentRequiredAsync(_ct)).Returns(owner);
        A.CallTo(() => _gameRepository.GetByIdAsync(A<GameId>._, _ct)).Returns(game);

        // Act
        var act = () => _guard.EnsureOwnerAsync(game.Id, _ct);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task EnsureOwnerAsync_WhenGameNotFound_ThrowsGameNotFound()
    {
        // Arrange
        var owner = OwnerWithId(out _);
        A.CallTo(() => _playerService.GetCurrentRequiredAsync(_ct)).Returns(owner);
        A.CallTo(() => _gameRepository.GetByIdAsync(A<GameId>._, _ct)).Returns((Game?)null);

        // Act
        var act = () => _guard.EnsureOwnerAsync(new GameId(Guid.NewGuid()), _ct);

        // Assert
        await act.Should().ThrowAsync<GameNotFoundException>();
    }

    [Fact]
    public async Task EnsureOwnerAsync_WhenCallerHasNoProfile_PropagatesForbiddenWithoutLoadingGame()
    {
        // Arrange — GetCurrentRequiredAsync throws for an unprovisioned caller.
        A.CallTo(() => _playerService.GetCurrentRequiredAsync(_ct))
            .Throws(new ForbiddenAccessException("no profile"));

        // Act
        var act = () => _guard.EnsureOwnerAsync(new GameId(Guid.NewGuid()), _ct);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
        A.CallTo(() => _gameRepository.GetByIdAsync(A<GameId>._, _ct)).MustNotHaveHappened();
    }
}
