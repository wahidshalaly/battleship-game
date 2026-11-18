using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using Microsoft.Extensions.Options;

namespace BattleshipGame.UnitTests.Application.Features.Players.Queries;

public class GetLeaderboardHandlerTests
{
    private const int MinimumFinishedGamesToRank = 5;

    private readonly ILeaderboardReader _reader = A.Fake<ILeaderboardReader>();
    private readonly GetLeaderboardHandler _handler;
    private readonly CancellationToken _ct = CancellationToken.None;

    public GetLeaderboardHandlerTests() =>
        _handler = new GetLeaderboardHandler(
            _reader,
            Options.Create(
                new LeaderboardOptions { MinimumFinishedGamesToRank = MinimumFinishedGamesToRank }
            )
        );

    [Fact]
    public async Task Handle_ShouldReadTheRequestedNumberOfPlayersWithTheConfiguredMinimum()
    {
        // Arrange
        ArrangeStandings();

        // Act
        var result = await _handler.Handle(new GetLeaderboardQuery(Top: 7), _ct);

        // Assert
        result.MinimumFinishedGamesToRank.Should().Be(MinimumFinishedGamesToRank);
        A.CallTo(() => _reader.GetTopPlayersAsync(7, MinimumFinishedGamesToRank, _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ShouldRankPlayersInTheOrderTheReaderReturnsThem()
    {
        // Arrange
        var first = Standing("first", won: 4, lost: 1);
        var second = Standing("second", won: 1, lost: 0);
        ArrangeStandings(first, second);

        // Act
        var result = await _handler.Handle(new GetLeaderboardQuery(Top: 10), _ct);

        // Assert
        result
            .Entries.Select(e => (e.Rank, e.Username))
            .Should()
            .Equal((1, "first"), (2, "second"));
    }

    [Fact]
    public async Task Handle_ShouldMapEachPlayersStatistics()
    {
        // Arrange
        var standing = Standing("player", won: 3, lost: 1, aiGamesWon: 2);
        ArrangeStandings(standing);

        // Act
        var result = await _handler.Handle(new GetLeaderboardQuery(Top: 10), _ct);

        // Assert
        var entry = result.Entries.Should().ContainSingle().Subject;
        entry.PlayerId.Should().Be(standing.PlayerId);
        entry.GamesFinished.Should().Be(4);
        entry.GamesWon.Should().Be(3);
        entry.AiGamesWon.Should().Be(2);
        entry.WinRate.Should().Be(0.75);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    public async Task Handle_ShouldMarkPlayersBelowTheMinimumAsProvisional(
        int gamesFinished,
        bool expectedProvisional
    )
    {
        // Arrange
        ArrangeStandings(Standing("player", won: 1, lost: gamesFinished - 1));

        // Act
        var result = await _handler.Handle(new GetLeaderboardQuery(Top: 10), _ct);

        // Assert
        result.Entries.Single().IsProvisional.Should().Be(expectedProvisional);
    }

    [Fact]
    public async Task Handle_WhenNoPlayerHasFinishedAGame_ShouldReturnNoEntries()
    {
        // Arrange
        ArrangeStandings();

        // Act
        var result = await _handler.Handle(new GetLeaderboardQuery(Top: 10), _ct);

        // Assert
        result.Entries.Should().BeEmpty();
    }

    private void ArrangeStandings(params PlayerStanding[] standings) =>
        A.CallTo(() => _reader.GetTopPlayersAsync(A<int>._, A<int>._, A<CancellationToken>._))
            .Returns(new List<PlayerStanding>(standings));

    private static PlayerStanding Standing(
        string username,
        int won,
        int lost,
        int aiGamesWon = 0
    ) => new(new PlayerId(Guid.NewGuid()), username, new GameResults(won, lost), aiGamesWon);
}
