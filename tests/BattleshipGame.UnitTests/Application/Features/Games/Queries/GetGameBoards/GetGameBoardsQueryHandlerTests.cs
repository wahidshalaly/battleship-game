using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Application.Features.Games.Queries.GetGameBoards;

public class GetGameBoardsQueryHandlerTests
{
    private readonly IGameRepository _gameRepository;
    private readonly GetGameBoardsHandler _handler;
    private readonly GameFixture _gameFixture = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetGameBoardsQueryHandlerTests()
    {
        _gameRepository = A.Fake<IGameRepository>();
        _handler = new GetGameBoardsHandler(_gameRepository);
    }

    [Fact]
    public async Task Handle_WhenGameDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        A.CallTo(() => _gameRepository.GetByIdAsync(gameId, _cancellationToken))
            .Returns<Game?>(null);

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(gameId), _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenGameIsNew_ShouldReturnEmptyBoards()
    {
        // Arrange
        var game = ArrangeGame(_gameFixture.CreateGameInStateNew());

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(game.Id.Value);
        result.OwnBoard.Should().BeEquivalentTo(new BoardView([], [], []));
        result.OpponentBoard.Should().BeEquivalentTo(new BoardView([], [], []));
    }

    [Fact]
    public async Task Handle_ShouldReturnEveryOwnShipWithItsPosition()
    {
        // Arrange
        var game = ArrangeGame(_gameFixture.CreateGameInStateReady());
        var expectedShips = game.GetShips(BoardSide.Player)
            .Select(id => new ShipView(
                game.GetShipKind(BoardSide.Player, id),
                [.. game.GetShipPosition(BoardSide.Player, id)],
                Sunk: false
            ));

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.OwnBoard.Ships.Should().HaveCount(ShipAllowance);
        result.OwnBoard.Ships.Should().BeEquivalentTo(expectedShips);
    }

    [Fact]
    public async Task Handle_ShouldHideOpponentShipsThatAreNotSunk()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var battleship = GetShipOfKind(game, BoardSide.Opponent, ShipKind.Battleship);
        game.Attack(
            BoardSide.Opponent,
            game.GetShipPosition(BoardSide.Opponent, battleship).First()
        );
        ArrangeGame(game);

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.OpponentBoard.Ships.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRevealOpponentShipsThatAreSunk()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var destroyer = GetShipOfKind(game, BoardSide.Opponent, ShipKind.Destroyer);
        _gameFixture.AttackShip(game, BoardSide.Opponent, destroyer);
        ArrangeGame(game);

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result
            .OpponentBoard.Ships.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new ShipView(
                    ShipKind.Destroyer,
                    [.. game.GetShipPosition(BoardSide.Opponent, destroyer)],
                    Sunk: true
                )
            );
    }

    [Fact]
    public async Task Handle_ShouldMarkOwnShipsThatAreSunk()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        var destroyer = GetShipOfKind(game, BoardSide.Player, ShipKind.Destroyer);
        _gameFixture.AttackShip(game, BoardSide.Player, destroyer);
        ArrangeGame(game);

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.OwnBoard.Ships.Should().HaveCount(ShipAllowance);
        result
            .OwnBoard.Ships.Should()
            .ContainSingle(s => s.Sunk)
            .Which.Kind.Should()
            .Be(ShipKind.Destroyer);
    }

    [Fact]
    public async Task Handle_ShouldReturnHitsAndMissesOfEachBoard()
    {
        // Arrange
        var game = _gameFixture.CreateGameInStateStarted();
        game.Attack(BoardSide.Opponent, "A1");
        game.Attack(BoardSide.Player, "J10");
        game.Attack(BoardSide.Opponent, "J9");
        game.Attack(BoardSide.Player, "B1");
        ArrangeGame(game);

        // Act
        var result = await _handler.Handle(new GetGameBoardsQuery(game.Id), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.OpponentBoard.Hits.Should().BeEquivalentTo(["A1"]);
        result.OpponentBoard.Misses.Should().BeEquivalentTo(["J9"]);
        result.OwnBoard.Hits.Should().BeEquivalentTo(["B1"]);
        result.OwnBoard.Misses.Should().BeEquivalentTo(["J10"]);
    }

    private Game ArrangeGame(Game game)
    {
        A.CallTo(() => _gameRepository.GetByIdAsync(game.Id, _cancellationToken)).Returns(game);
        return game;
    }

    private static ShipId GetShipOfKind(Game game, BoardSide boardSide, ShipKind kind) =>
        game.GetShips(boardSide).Single(id => game.GetShipKind(boardSide, id) == kind);
}
