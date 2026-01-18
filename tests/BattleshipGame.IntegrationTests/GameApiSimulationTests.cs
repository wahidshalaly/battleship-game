using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

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

    [Theory]
    [InlineData(OpponentStrategy.Random)]
    [InlineData(OpponentStrategy.SemanticKernel)]
    public async Task Simulate_Full_Game_Playthrough_Via_Api(OpponentStrategy strategy)
    {
        const string playerUsername = "testuser";

        // 1. Create player
        var playerId = await CreatePlayer(playerUsername);

        // 2. Create game with selected opponent strategy
        var gameId = await CreateGame(playerId, strategy);
        await VerifyGameState(gameId, GameState.New);

        // 3. Place ships for both sides
        await PlaceShips(gameId);
        await VerifyGameState(gameId, GameState.Ready);

        // 4. Start gameplay
        await StartGameplay(gameId);
        await VerifyGameState(gameId, GameState.Started);

        // 5. Attack all Opponent ship positions
        await AttackShips(gameId);
        await VerifyGameState(gameId, GameState.GameOver);
    }

    private async Task VerifyGameState(Guid gameId, GameState gameState)
    {
        var getGameResult = await _client.GetFromJsonAsync<GetGameQueryResult>(
            $"/api/games/{gameId}"
        );
        getGameResult.Should().NotBeNull();
        getGameResult.State.Should().Be(gameState);
    }

    private static (
        string BowCode,
        ShipKind Kind,
        ShipOrientation Orientation,
        string[] Position
    )[] DefineShips(Guid gameId)
    {
        return
        [
            ("A1", ShipKind.Destroyer, ShipOrientation.Horizontal, ["A1", "B1"]),
            ("B2", ShipKind.Submarine, ShipOrientation.Vertical, ["B2", "B3", "B4"]),
            ("C3", ShipKind.Cruiser, ShipOrientation.Horizontal, ["C3", "D3", "E3"]),
            ("D4", ShipKind.Battleship, ShipOrientation.Vertical, ["D4", "D5", "D6", "D7"]),
            ("E5", ShipKind.Carrier, ShipOrientation.Horizontal, ["E5", "F5", "G5", "H5", "I5"]),
        ];
    }

    private async Task PlaceShips(Guid gameId)
    {
        var shipDefs = DefineShips(gameId);

        foreach (var (bowCode, kind, orientation, _) in shipDefs)
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

    private async Task AttackShips(Guid gameId)
    {
        var shipDefs = DefineShips(gameId);

        foreach (var (_, _, _, position) in shipDefs)
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

    private async Task<Guid> CreateGame(Guid playerId, OpponentStrategy opponentStrategy)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/games",
            new
            {
                PlayerId = playerId,
                BoardSize = 10,
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
