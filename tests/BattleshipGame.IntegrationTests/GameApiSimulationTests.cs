using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.IntegrationTests;

public class GameApiSimulationTests(
    ITestOutputHelper output,
    WebApplicationFactory<Program> factory
) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(new XunitLoggerProvider(output));
                logging.SetMinimumLevel(LogLevel.Debug);
            });
        })
        .CreateClient();

    private const int BoardSize = DefaultBoardSize;

    [Theory]
    [InlineData(OpponentStrategy.Random)]
    [InlineData(OpponentStrategy.SemanticKernel)]
    public async Task Simulate_Full_Game_Playthrough_Via_Api(OpponentStrategy strategy)
    {
        const string playerUsername = "testuser";

        // 1. Create player
        var playerId = await CreatePlayer(playerUsername);

        // 2. Create game with selected opponent strategy
        var gameId = await CreateGame(playerId, BoardSize, strategy);
        await VerifyGameState(gameId, GameState.New);

        // 3. Generate random ship placements
        var shipPlacements = GenerateRandomShipPlacements(BoardSize);

        // 4. Place ships for both sides
        await PlaceShips(gameId, shipPlacements);
        await VerifyGameState(gameId, GameState.Ready);

        // 5. Start gameplay
        await StartGameplay(gameId);
        await VerifyGameState(gameId, GameState.Started);

        // 6. Attack all Opponent ship positions
        await AttackShips(gameId, shipPlacements);
        await VerifyGameState(gameId, GameState.GameOver);
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
        )[] shipPlacements
    )
    {
        foreach (var (bowCode, kind, orientation, _) in shipPlacements)
        {
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new PlaceShipRequest(BoardSide.Player, kind, orientation, bowCode)
            );
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new PlaceShipRequest(BoardSide.Opponent, kind, orientation, bowCode)
            );
        }
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
