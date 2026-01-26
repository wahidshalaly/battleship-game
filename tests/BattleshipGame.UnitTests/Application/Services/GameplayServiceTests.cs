using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Services;

public class GameplayServiceTests
{
    private readonly GameFixture _fixture = new();
    private readonly IMediator _mediator;
    private readonly GameplayService _subject;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GameplayServiceTests()
    {
        _mediator = A.Fake<IMediator>();
        _subject = new GameplayService(_mediator);
    }

    [Fact]
    public async Task PlayerAttackThenCounterAttackAsync_WhenBothAttacksSucceed_ShouldReturnCompleteResult()
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        var cellCode = "A1";

        var playerAttackResult = new AttackResult(
            TargetCell: "A1",
            CellState: CellState.Hit,
            GameState: GameState.Started,
            WinnerSide: BoardSide.None,
            SunkShip: null,
            ShipSize: null
        );

        var opponentAttackResult = new AttackResult(
            TargetCell: "B2",
            CellState: CellState.Missed,
            GameState: GameState.Started,
            WinnerSide: BoardSide.None,
            SunkShip: null,
            ShipSize: null
        );

        A.CallTo(() =>
                _mediator.Send(
                    A<PlayerAttackCommand>.That.Matches(cmd =>
                        cmd.GameId == gameId && cmd.CellCode == cellCode
                    ),
                    _cancellationToken
                )
            )
            .Returns(playerAttackResult);

        A.CallTo(() =>
                _mediator.Send(
                    A<OpponentAttackCommand>.That.Matches(cmd => cmd.GameId == gameId),
                    _cancellationToken
                )
            )
            .Returns(opponentAttackResult);

        // Act
        var result = await _subject.PlayerAttackThenCounterAttackAsync(
            gameId,
            cellCode,
            _cancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(gameId);
        result.PlayerTargetCell.Should().Be("A1");
        result.PlayerAttackResult.Should().Be(CellState.Hit);
        result.OpponentTargetCell.Should().Be("B2");
        result.OpponentAttackResult.Should().Be(CellState.Missed);
        result.GameState.Should().Be(GameState.Started);
    }

    [Fact]
    public async Task PlayerAttackThenCounterAttackAsync_WhenPlayerWins_ShouldNotExecuteOpponentAttack()
    {
        // Arrange
        var gameId = new GameId(Guid.NewGuid());
        var cellCode = "A1";

        var playerAttackResult = new AttackResult(
            TargetCell: "A1",
            CellState: CellState.Hit,
            GameState: GameState.GameOver,
            WinnerSide: BoardSide.Player,
            SunkShip: ShipKind.Carrier,
            ShipSize: 5
        );

        A.CallTo(() =>
                _mediator.Send(
                    A<PlayerAttackCommand>.That.Matches(cmd => cmd.GameId == gameId),
                    _cancellationToken
                )
            )
            .Returns(playerAttackResult);

        // Act
        var result = await _subject.PlayerAttackThenCounterAttackAsync(
            gameId,
            cellCode,
            _cancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.PlayerTargetCell.Should().Be("A1");
        result.OpponentTargetCell.Should().BeNull();
        result.OpponentAttackResult.Should().BeNull();
        result.GameState.Should().Be(GameState.GameOver);
        result.WinnerSide.Should().Be(BoardSide.Player);

        // Verify opponent attack was NOT executed
        A.CallTo(() => _mediator.Send(A<OpponentAttackCommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
