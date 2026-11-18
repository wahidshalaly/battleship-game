using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence.Queries;
using BattleshipGame.Infrastructure.Persistence.Repositories;
using BattleshipGame.IntegrationTests.Infrastructure.Persistence.Repositories;

namespace BattleshipGame.IntegrationTests.Infrastructure.Persistence.Queries;

/// <remarks>
/// The database is shared with every other test, so the leaderboard holds their players too.
/// Each test reads the whole board and looks only at the players it seeded.
/// </remarks>
[Collection(PostgresCollection.Name)]
public class LeaderboardReaderTests(PostgresFixture postgres) : BaseRepositoryTests(postgres)
{
    private const int MinimumFinishedGamesToRank = 5;
    private const int WholeBoard = int.MaxValue;

    private readonly GameFixture _fixture = new();
    private readonly string _usernamePrefix = $"lb{Guid.NewGuid():N}"[..12];
    private readonly CancellationToken _ct = CancellationToken.None;
    private LeaderboardReader _subject = null!;
    private GameRepository _games = null!;
    private PlayerRepository _players = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _subject = new LeaderboardReader(_dbcontext);
        _games = new GameRepository(_dbcontext);
        _players = new PlayerRepository(_dbcontext);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ShouldCountFinishedGamesWinsAndAiWins()
    {
        // Arrange
        var player = await SeedPlayerAsync("counts");
        await SeedFinishedGamesAsync(player, won: 2, lost: 1, wonAgainstAi: 1);
        await _games.SaveAsync(_fixture.CreateGameInStateStarted(player), _ct);
        await CommitAsync();

        // Act
        var standings = await _subject.GetTopPlayersAsync(
            WholeBoard,
            MinimumFinishedGamesToRank,
            _ct
        );

        // Assert
        var standing = standings.Should().ContainSingle(s => s.PlayerId == player).Subject;
        standing.Username.Should().Be(UsernameOf("counts"));
        standing.Results.Should().Be(new GameResults(GamesWon: 3, GamesLost: 1));
        standing.AiGamesWon.Should().Be(1);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ShouldLeaveOutPlayersWithoutFinishedGames()
    {
        // Arrange
        var withActiveGame = await SeedPlayerAsync("active");
        await _games.SaveAsync(_fixture.CreateGameInStateStarted(withActiveGame), _ct);
        var withoutGames = await SeedPlayerAsync("idle");
        await CommitAsync();

        // Act
        var standings = await _subject.GetTopPlayersAsync(
            WholeBoard,
            MinimumFinishedGamesToRank,
            _ct
        );

        // Assert
        standings.Should().NotContain(s => s.PlayerId == withActiveGame);
        standings.Should().NotContain(s => s.PlayerId == withoutGames);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ShouldRankRankedPlayersByWinRateThenAiWinsThenWinsThenUsername()
    {
        // Arrange
        var best = await SeedPlayerAsync("a_best");
        await SeedFinishedGamesAsync(best, won: 4, lost: 1);
        var aiWinner = await SeedPlayerAsync("b_ai");
        await SeedFinishedGamesAsync(aiWinner, won: 1, lost: 2, wonAgainstAi: 2);
        var moreWins = await SeedPlayerAsync("c_more");
        await SeedFinishedGamesAsync(moreWins, won: 6, lost: 4);
        var tiedFirst = await SeedPlayerAsync("d_tied1");
        await SeedFinishedGamesAsync(tiedFirst, won: 3, lost: 2);
        var tiedSecond = await SeedPlayerAsync("d_tied2");
        await SeedFinishedGamesAsync(tiedSecond, won: 3, lost: 2);
        await CommitAsync();

        // Act
        var standings = await _subject.GetTopPlayersAsync(
            WholeBoard,
            MinimumFinishedGamesToRank,
            _ct
        );

        // Assert
        SeededPlayersIn(standings).Should().Equal(best, aiWinner, moreWins, tiedFirst, tiedSecond);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ShouldPlaceProvisionalPlayersAfterRankedOnes()
    {
        // Arrange
        var luckyNewcomer = await SeedPlayerAsync("a_lucky");
        await SeedFinishedGamesAsync(luckyNewcomer, won: 1, lost: 0);
        var ranked = await SeedPlayerAsync("b_ranked");
        await SeedFinishedGamesAsync(ranked, won: 1, lost: 4);
        var unluckyNewcomer = await SeedPlayerAsync("c_unlucky");
        await SeedFinishedGamesAsync(unluckyNewcomer, won: 0, lost: 2);
        await CommitAsync();

        // Act
        var standings = await _subject.GetTopPlayersAsync(
            WholeBoard,
            MinimumFinishedGamesToRank,
            _ct
        );

        // Assert
        SeededPlayersIn(standings).Should().Equal(ranked, luckyNewcomer, unluckyNewcomer);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ShouldReturnAtMostTheRequestedNumberOfPlayers()
    {
        // Arrange
        await SeedFinishedGamesAsync(await SeedPlayerAsync("one"), won: 1, lost: 0);
        await SeedFinishedGamesAsync(await SeedPlayerAsync("two"), won: 1, lost: 0);
        await CommitAsync();

        // Act
        var standings = await _subject.GetTopPlayersAsync(1, MinimumFinishedGamesToRank, _ct);

        // Assert
        standings.Should().ContainSingle();
    }

    private string UsernameOf(string name) => $"{_usernamePrefix}_{name}";

    private async Task<PlayerId> SeedPlayerAsync(string name)
    {
        var player = new Player(
            new PlayerId(Guid.NewGuid()),
            UsernameOf(name),
            $"sub|{Guid.NewGuid()}"
        );
        return await _players.SaveAsync(player, _ct);
    }

    // Wins against the AI opponent come on top of the wins against Random.
    private async Task SeedFinishedGamesAsync(
        PlayerId player,
        int won,
        int lost,
        int wonAgainstAi = 0
    )
    {
        for (var i = 0; i < won; i++)
            await _games.SaveAsync(
                _fixture.CreateGameInStateGameOver(player, BoardSide.Player),
                _ct
            );
        for (var i = 0; i < wonAgainstAi; i++)
            await _games.SaveAsync(
                _fixture.CreateGameInStateGameOver(
                    player,
                    BoardSide.Player,
                    OpponentStrategy.SemanticKernel
                ),
                _ct
            );
        for (var i = 0; i < lost; i++)
            await _games.SaveAsync(
                _fixture.CreateGameInStateGameOver(player, BoardSide.Opponent),
                _ct
            );
    }

    private List<PlayerId> SeededPlayersIn(IEnumerable<PlayerStanding> standings) =>
        [.. standings.Where(s => s.Username.StartsWith(_usernamePrefix)).Select(s => s.PlayerId)];
}
