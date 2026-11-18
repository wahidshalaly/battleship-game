using System.Net;
using System.Text.Json;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.Persistence;
using BattleshipGame.Infrastructure.Persistence.Entities;
using BattleshipGame.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class GameApiSimulationTests(ITestOutputHelper output, PostgresFixture postgres)
    : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _clientSubject = null!;
    private readonly FakeLlmChat _chat = new();

    private const int BoardSize = DefaultBoardSize;

    public Task InitializeAsync()
    {
        _factory = new BattleshipWebApplicationFactory(
            postgres.ConnectionString,
            _chat
        ).WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(new XunitLoggerProvider(output));
                logging.SetMinimumLevel(LogLevel.Debug);
            });
        });
        _client = _factory.CreateClient();
        // Authenticate every request via the test auth scheme (see TestAuthHandler).
        _clientSubject = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, _clientSubject);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();

        // A missing script would otherwise pass silently via the Random fallback.
        _chat.VerifyAllCallsScripted();
    }

    [Fact]
    public async Task Request_WithoutAuthentication_IsRejectedWith401()
    {
        // A client that does not send the test subject header is treated as anonymous.
        using var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync($"/api/games/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGame_WhenCallerHasNoPlayerProfile_IsForbidden()
    {
        // Authenticated, but has not registered a player profile.
        using var client = CreateAuthenticatedClient(Guid.NewGuid().ToString());

        var response = await client.PostAsJsonAsync(
            "/api/games",
            new { BoardSize, OpponentStrategy = OpponentStrategy.Random }
        );

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGame_WhenCallerIsNotOwner_IsForbidden()
    {
        // The owner (_client) has a seeded player profile.
        await SeedPlayer(_clientSubject, $"owner_{Guid.NewGuid():N}"[..32]);
        var gameId = await CreateGame(BoardSize, OpponentStrategy.Random);

        // A different authenticated player who also has a profile.
        var otherSubject = Guid.NewGuid().ToString();
        using var other = CreateAuthenticatedClient(otherSubject);
        await SeedPlayer(otherSubject, $"other_{Guid.NewGuid():N}"[..32]);

        // The non-owner cannot access the owner's game.
        var response = await other.GetAsync($"/api/games/{gameId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(OpponentStrategy.Random)]
    [InlineData(OpponentStrategy.SemanticKernel)]
    public async Task Simulate_Full_Game_Playthrough_Via_Api(OpponentStrategy strategy)
    {
        // The SemanticKernel opponent plays a scripted sequence of distinct cells, so its moves
        // are predictable. Any fallback to Random would break the sequence.
        var scriptedOpponentCells = AllCells(BoardSize);
        if (strategy == OpponentStrategy.SemanticKernel)
            _chat.ReplyWithCells(scriptedOpponentCells);

        // Username must be 3-32 chars, letters/digits/underscore only.
        var playerUsername = $"u_{strategy.ToString().ToLower()}_{Guid.NewGuid():N}"[..32];

        // 1. Seed the caller's player profile directly into the DB.
        await SeedPlayer(_clientSubject, playerUsername);

        // 2. Create game with selected opponent strategy (owned by the caller)
        var gameId = await CreateGame(BoardSize, strategy);
        await VerifyGameState(gameId, GameState.New);

        // 3. Generate independent ship placements for each side.
        var opponentPlacements = GenerateRandomShipPlacements(BoardSize);
        var playerPlacements = GenerateRandomShipPlacements(BoardSize);

        // 4. Place ships for both sides
        await PlaceShips(gameId, playerPlacements, opponentPlacements);
        await VerifyGameState(gameId, GameState.Ready);

        // 5. Start gameplay
        await StartGameplay(gameId);
        await VerifyGameState(gameId, GameState.Started);

        // 6. Attack all Opponent ship positions until the game ends
        var opponentCells = await AttackShips(gameId, opponentPlacements);
        await VerifyGameState(gameId, GameState.GameOver);

        // 7. Every SemanticKernel move came from the (fake) model, in order: no fallbacks.
        if (strategy == OpponentStrategy.SemanticKernel)
        {
            opponentCells.Should().Equal(scriptedOpponentCells.Take(opponentCells.Count));
            _chat.CallCount.Should().Be(opponentCells.Count);
        }
        else
        {
            _chat.CallCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task FinishedGame_IsNoLongerActive_AndPlayerCanStartAnotherGame()
    {
        await SeedPlayer(_clientSubject, $"u_done_{Guid.NewGuid():N}"[..32]);
        var finishedGameId = await PlayRandomGameToGameOver();

        var activeResponse = await _client.GetAsync("/api/games/active");
        activeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var me = await GetMe();
        me.ActiveGameId.Should().BeNull();
        me.TotalGamesPlayed.Should().Be(1);

        var newGameId = await CreateGame(BoardSize, OpponentStrategy.Random);
        newGameId.Should().NotBe(finishedGameId);
        var active = await _client.GetFromJsonAsync<GetGameQueryResult>("/api/games/active");
        active!.GameId.Should().Be(newGameId);
        (await GetMe()).TotalGamesPlayed.Should().Be(2);
    }

    [Fact]
    public async Task CreateGame_WhilePlayerHasActiveGame_IsRejectedWithConflict()
    {
        await SeedPlayer(_clientSubject, $"u_busy_{Guid.NewGuid():N}"[..32]);
        var activeGameId = await CreateGame(BoardSize, OpponentStrategy.Random);

        var response = await _client.PostAsJsonAsync(
            "/api/games",
            new { BoardSize, OpponentStrategy = OpponentStrategy.Random }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("activeGameId").GetGuid().Should().Be(activeGameId);
        var active = await _client.GetFromJsonAsync<GetGameQueryResult>("/api/games/active");
        active!.GameId.Should().Be(activeGameId);
        (await GetMe()).TotalGamesPlayed.Should().Be(1);
    }

    [Fact]
    public async Task PlayerAttack_WhenAiReplyIsNotValidJson_RetriesThenFallsBackToRandom()
    {
        _chat.ReplyRaw("I think B7 looks promising!");

        var (_, roundResult) = await PlayFirstRoundAgainstSemanticKernel();

        AssertOpponentStillPlayed(roundResult);
        // One attempt plus one retry (Resilience:AiOpponent:MaxRetryAttempts = 1 in tests).
        _chat.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task PlayerAttack_WhenAiIsRateLimited_RetriesThenFallsBackToRandom()
    {
        _chat.Throw(
            new HttpRequestException("Too many requests", null, HttpStatusCode.TooManyRequests)
        );

        var (_, roundResult) = await PlayFirstRoundAgainstSemanticKernel();

        AssertOpponentStillPlayed(roundResult);
        _chat.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task PlayerAttack_WhenAiFailsUnexpectedly_FallsBackToRandomWithoutRetry()
    {
        _chat.Throw(
            new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError)
        );

        var (gameId, roundResult) = await PlayFirstRoundAgainstSemanticKernel();

        AssertOpponentStillPlayed(roundResult);
        _chat.CallCount.Should().Be(1);

        // The game stays playable after a fallback.
        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/attacks",
            new AttackRequest("A2")
        );
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Sets up a started SemanticKernel game and plays the first round (player attacks A1).
    private async Task<(Guid GameId, LastRoundResult Round)> PlayFirstRoundAgainstSemanticKernel()
    {
        await SeedPlayer(_clientSubject, $"resilience_test_{Guid.NewGuid():N}"[..32]);
        var gameId = await CreateGame(BoardSize, OpponentStrategy.SemanticKernel);
        var shipPlacements = GenerateRandomShipPlacements(BoardSize);
        await PlaceShips(gameId, shipPlacements, shipPlacements);
        await StartGameplay(gameId);

        var response = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/attacks",
            new AttackRequest("A1")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roundResult = await response.Content.ReadFromJsonAsync<LastRoundResult>();
        roundResult.Should().NotBeNull();
        roundResult!.PlayerTargetCell.Should().Be("A1");
        return (gameId, roundResult);
    }

    private static void AssertOpponentStillPlayed(LastRoundResult roundResult)
    {
        roundResult
            .OpponentTargetCell.Should()
            .NotBeNullOrWhiteSpace("the opponent should never forfeit its turn");
        roundResult.OpponentAttackResult.Should().NotBeNull();
        roundResult.GameState.Should().Be(GameState.Started);
    }

    // Every cell on the board in a fixed order (A1, B1, ... then A2, ...); never repeats.
    private static List<string> AllCells(int boardSize) =>
        [
            .. Enumerable
                .Range(1, boardSize)
                .SelectMany(row =>
                    Enumerable.Range(0, boardSize).Select(column => $"{(char)('A' + column)}{row}")
                ),
        ];

    // Inserts a Player row directly into the DB so the test auth subject has an owner profile.
    // This replaces the removed POST /api/players endpoint (registration is now POST /api/auth/register).
    private async Task<Guid> PlayRandomGameToGameOver()
    {
        var gameId = await CreateGame(BoardSize, OpponentStrategy.Random);
        var opponentPlacements = GenerateRandomShipPlacements(BoardSize);
        await PlaceShips(gameId, GenerateRandomShipPlacements(BoardSize), opponentPlacements);
        await StartGameplay(gameId);
        await AttackShips(gameId, opponentPlacements);
        await VerifyGameState(gameId, GameState.GameOver);
        return gameId;
    }

    private async Task<PlayerResponse> GetMe()
    {
        var me = await _client.GetFromJsonAsync<PlayerResponse>("/api/players/me");
        me.Should().NotBeNull();
        return me;
    }

    private async Task SeedPlayer(string subject, string username)
    {
        await using var db = postgres.CreateDbContext();
        db.Players.Add(
            new PlayerEntity
            {
                Id = Guid.NewGuid(),
                Username = username,
                IdentitySubject = subject,
            }
        );
        await db.SaveChangesAsync();
    }

    private HttpClient CreateAuthenticatedClient(string subject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubjectHeader, subject);
        return client;
    }

    private async Task<GetGameQueryResult> GetGame(Guid gameId)
    {
        var result = await _client.GetFromJsonAsync<GetGameQueryResult>($"/api/games/{gameId}");
        result.Should().NotBeNull();
        return result;
    }

    private async Task VerifyGameState(Guid gameId, GameState gameState)
    {
        var result = await GetGame(gameId);
        result.State.Should().Be(gameState);
    }

    private (
        string BowCode,
        ShipKind Kind,
        ShipOrientation Orientation,
        string[] Position
    )[] GenerateRandomShipPlacements(int boardSize)
    {
        var generator = new RandomShipPlacementGenerator(boardSize);
        var placements = generator.GeneratePlacements();

        foreach (var (bowCode, kind, orientation, position) in placements)
        {
            output.WriteLine(
                "Ship {0}: Bow={1}, Orientation={2}, Position=[{3}]",
                kind,
                bowCode,
                orientation,
                string.Join(", ", position)
            );
        }

        return placements;
    }

    private async Task PlaceShips(
        Guid gameId,
        (
            string BowCode,
            ShipKind Kind,
            ShipOrientation Orientation,
            string[] Position
        )[] playerPlacements,
        (
            string BowCode,
            ShipKind Kind,
            ShipOrientation Orientation,
            string[] Position
        )[] opponentPlacements
    )
    {
        foreach (var (bowCode, kind, orientation, _) in playerPlacements)
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new PlaceShipRequest(BoardSide.Player, kind, orientation, bowCode)
            );

        foreach (var (bowCode, kind, orientation, _) in opponentPlacements)
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new PlaceShipRequest(BoardSide.Opponent, kind, orientation, bowCode)
            );
    }

    private async Task StartGameplay(Guid gameId)
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/games/{gameId}/state",
            new { state = GameState.Started }
        );
        response.EnsureSuccessStatusCode();
    }

    // Attacks the given ship cells until the game ends; returns the opponent's counter-attacks.
    private async Task<List<string>> AttackShips(
        Guid gameId,
        (
            string BowCode,
            ShipKind Kind,
            ShipOrientation Orientation,
            string[] Position
        )[] shipPlacements
    )
    {
        var opponentCells = new List<string>();
        foreach (var (_, _, _, position) in shipPlacements)
        {
            foreach (var cellCode in position)
            {
                var response = await _client.PostAsJsonAsync(
                    $"/api/games/{gameId}/attacks",
                    new AttackRequest(cellCode)
                );
                response.EnsureSuccessStatusCode();
                var roundResult = await response.Content.ReadFromJsonAsync<LastRoundResult>();
                output.WriteLine("Attacked {0}. Outcome: {1}", cellCode, roundResult);
                if (roundResult?.OpponentTargetCell is { } opponentCell)
                    opponentCells.Add(opponentCell);
                if (roundResult?.GameState == GameState.GameOver)
                    return opponentCells;
            }
        }

        return opponentCells;
    }

    private async Task<Guid> CreateGame(int boardSize, OpponentStrategy opponentStrategy)
    {
        // The owner is taken from the authenticated caller, not the request body.
        var response = await _client.PostAsJsonAsync(
            "/api/games",
            new { BoardSize = boardSize, OpponentStrategy = opponentStrategy }
        );
        response.EnsureSuccessStatusCode();
        var createdGameLocation = response.Headers.Location;
        createdGameLocation.Should().NotBeNull();
        var gameId = ExtractIdFromLocation(createdGameLocation);
        return gameId;
    }

    private static Guid ExtractIdFromLocation(Uri location)
    {
        var segments = location.Segments;
        return Guid.Parse(segments[^1]);
    }
}
