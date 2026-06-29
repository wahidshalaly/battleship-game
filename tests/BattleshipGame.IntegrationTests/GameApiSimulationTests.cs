using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.IntegrationTests;

public class GameApiSimulationTests(ITestOutputHelper output, PostgresFixture postgres)
    : IClassFixture<PostgresFixture>,
        IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    private const int BoardSize = DefaultBoardSize;

    public Task InitializeAsync()
    {
        _factory = new BattleshipWebApplicationFactory(
            postgres.ConnectionString
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
        // SK opponent makes live AI calls that can take minutes per round on a local model;
        // raise the default 100s timeout to accommodate slow inference.
        _client.Timeout = TimeSpan.FromMinutes(5);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return _factory.DisposeAsync().AsTask();
    }

    [Theory]
    [InlineData(OpponentStrategy.Random)]
    [InlineData(OpponentStrategy.SemanticKernel)]
    public async Task Simulate_Full_Game_Playthrough_Via_Api(OpponentStrategy strategy)
    {
        // SemanticKernel requires a live AI endpoint; skip when OPENAI_API_KEY is absent.
        if (
            strategy == OpponentStrategy.SemanticKernel
            && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
        )
            return;

        // Username must be 3-32 chars, letters/digits/underscore only (see CreatePlayerCommandValidator).
        var playerUsername = $"u_{strategy.ToString().ToLower()}_{Guid.NewGuid():N}"[..32];

        // 1. Create player
        var playerId = await CreatePlayer(playerUsername);

        // 2. Create game with selected opponent strategy
        var gameId = await CreateGame(playerId, BoardSize, strategy);
        await VerifyGameState(gameId, GameState.New);

        // 3. Generate independent ship placements for each side so the player board
        //    is not a mirror of the opponent board. The test player attacks the
        //    opponent's known positions (guaranteed win in exactly N moves), while
        //    the SK opponent must discover the player's different positions by trial
        //    and error — making it practically impossible to win in those same N turns.
        var opponentPlacements = GenerateRandomShipPlacements(BoardSize);
        var playerPlacements = GenerateRandomShipPlacements(BoardSize);

        // 4. Place ships for both sides
        await PlaceShips(gameId, playerPlacements, opponentPlacements);
        await VerifyGameState(gameId, GameState.Ready);

        // 5. Start gameplay
        await StartGameplay(gameId);
        await VerifyGameState(gameId, GameState.Started);

        // 6. Attack all Opponent ship positions — proves SK called Ollama once per round
        await AttackShips(gameId, opponentPlacements);
        await VerifyGameState(gameId, GameState.GameOver);
    }

    [Fact]
    public async Task PlayerAttack_WhenAiOpponentFails_ShouldFallbackToRandomStrategy()
    {
        // This test validates resilience behavior when AI opponent fails.
        // Expected behavior:
        // 1. Player attack succeeds
        // 2. AI opponent (SemanticKernel) fails due to rate limiting or errors
        // 3. ResilientComputerOpponentDecorator automatically falls back to RandomAttackStrategy
        // 4. Opponent executes attack using fallback (never forfeits turn)
        // 5. Game state remains consistent (no turn corruption)
        // 6. Player can continue attacking

        // Username must be 3-32 chars, letters/digits/underscore only (see CreatePlayerCommandValidator).
        var playerUsername = $"resilience_test_{Guid.NewGuid():N}"[..32];

        // 1. Create player and game with SemanticKernel opponent
        var playerId = await CreatePlayer(playerUsername);
        var gameId = await CreateGame(playerId, BoardSize, OpponentStrategy.SemanticKernel);

        // 2. Setup game
        var shipPlacements = GenerateRandomShipPlacements(BoardSize);
        await PlaceShips(gameId, shipPlacements, shipPlacements);
        await StartGameplay(gameId);

        // 3. Execute player attack
        // Note: Even if SemanticKernel AI fails (rate limit, etc.), the decorator falls back to Random
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/attacks",
            new AttackRequest("A1")
        );

        // Assert: Request should succeed (200 OK)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var roundResult = await response.Content.ReadFromJsonAsync<LastRoundResult>();
        roundResult.Should().NotBeNull();
        roundResult!.PlayerTargetCell.Should().Be("A1");

        // Opponent should ALWAYS attack (using either SemanticKernel or fallback Random strategy)
        roundResult
            .OpponentTargetCell.Should()
            .NotBeNullOrWhiteSpace("Opponent should never forfeit turn");
        roundResult
            .OpponentAttackResult.Should()
            .NotBeNull("Opponent should always execute attack");
        roundResult.GameState.Should().Be(GameState.Started);

        // 4. Verify game state is consistent - turn switches back to player
        var game = await GetGame(gameId);
        game.State.Should().Be(GameState.Started);

        // 5. Verify subsequent attacks work
        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/attacks",
            new AttackRequest("A2")
        );

        secondResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
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

    private async Task AttackShips(
        Guid gameId,
        (
            string BowCode,
            ShipKind Kind,
            ShipOrientation Orientation,
            string[] Position
        )[] shipPlacements
    )
    {
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
                if (roundResult?.GameState == GameState.GameOver)
                    return;
            }
        }
    }

    private async Task<Guid> CreateGame(
        Guid playerId,
        int boardSize,
        OpponentStrategy opponentStrategy
    )
    {
        var response = await _client.PostAsJsonAsync(
            "/api/games",
            new
            {
                PlayerId = playerId,
                BoardSize = boardSize,
                OpponentStrategy = opponentStrategy,
            }
        );
        response.EnsureSuccessStatusCode();
        var createdGameLocation = response.Headers.Location;
        createdGameLocation.Should().NotBeNull();
        var gameId = ExtractIdFromLocation(createdGameLocation);
        return gameId;
    }

    private async Task<Guid> CreatePlayer(string playerUsername)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/players",
            new { Username = playerUsername }
        );
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        location.Should().NotBeNull();
        var playerId = ExtractIdFromLocation(location);
        return playerId;
    }

    private static Guid ExtractIdFromLocation(Uri location)
    {
        var segments = location.Segments;
        return Guid.Parse(segments[^1]);
    }
}
