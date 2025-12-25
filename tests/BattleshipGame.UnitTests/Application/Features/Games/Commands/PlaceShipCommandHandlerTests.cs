using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Games.Commands;

public class PlaceShipCommandHandlerTests
{
    private readonly IGameRepository _gameRepository;
    private readonly PlaceShipHandler _handler;

    public PlaceShipCommandHandlerTests()
    {
        _gameRepository = A.Fake<IGameRepository>();
        var eventDispatcher = A.Fake<IDomainEventDispatcher>();
        _handler = new PlaceShipHandler(_gameRepository, eventDispatcher);
    }

    [Theory]
    [MemberData(nameof(DifferentShipSetups))]
    public async Task Handle_WhenValidCommand_ShouldPlaceShipAndReturnResult(
        BoardSide boardSide,
        ShipKind shipKind,
        ShipOrientation orientation
    )
    {
        // Arrange
        const string bowCode = "A1";
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);
        var command = new PlaceShipCommand(game.Id, boardSide, shipKind, orientation, bowCode);
        var cancellationToken = CancellationToken.None;

        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, cancellationToken)).Returns(game);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenNoGameId_ShouldThrowGameNotFoundException()
    {
        // Arrange
        const string bowCode = "A1";
        var gameId = new GameId(Guid.NewGuid());
        var command = new PlaceShipCommand(
            gameId,
            BoardSide.Player,
            ShipKind.Battleship,
            ShipOrientation.Horizontal,
            bowCode
        );
        var cancellationToken = CancellationToken.None;

        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, cancellationToken))
            .Returns(Task.FromResult<Game?>(null));

        // Act
        var act = () => _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<GameNotFoundException>()
            .WithMessage($"Game `{gameId.Value}` is not found.");
    }

    public static System.Collections.Generic.IEnumerable<object[]> DifferentShipSetups()
    {
        return from BoardSide side in Enum.GetValues(typeof(BoardSide))
            from ShipKind kind in Enum.GetValues(typeof(ShipKind))
            from ShipOrientation orientation in Enum.GetValues(typeof(ShipOrientation))
            where
                side != BoardSide.None
                && kind != ShipKind.None
                && orientation != ShipOrientation.None
            select (object[])[side, kind, orientation];
    }
}
