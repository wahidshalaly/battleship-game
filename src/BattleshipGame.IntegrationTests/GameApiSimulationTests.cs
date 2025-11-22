using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BattleshipGame.IntegrationTests;

public class GameApiSimulationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string[] _bowCodes = ["A1", "B2", "C3", "D4", "E5"];

    [Fact]
    public async Task Simulate_Full_Game_Playthrough_Via_Api()
    {
        const string playerUsername = "testuser";

        // 1. Create player
        var playerId = await CreatePlayer(playerUsername);

        // 2. Create game
        var gameId = await CreateGame(playerId);
        await VerifyGameState(gameId, GameState.Started);

        // 3. Place ships for both sides
        await PlaceShips(gameId);
        await VerifyGameState(gameId, GameState.BoardsAreReady);

        // 4. Attack all Opp ship positions
        foreach (var code in _bowCodes)
        {
            await _client.PostAsJsonAsync($"/api/games/{gameId}/attacks", new AttackRequest(BoardSide.Opp, code));
        }
        await VerifyGameState(gameId, GameState.GameOver);
    }

    private async Task VerifyGameState(Guid gameId, GameState gameState)
    {
        var getGameResult = await _client.GetFromJsonAsync<GameModel>($"/api/games/{gameId}");
        getGameResult.Should().NotBeNull();
        getGameResult.State.Should().Be(gameState);
    }

    private async Task PlaceShips(Guid gameId)
    {
        var shipKinds = new[]
        {
            ShipKind.Destroyer,
            ShipKind.Submarine,
            ShipKind.Cruiser,
            ShipKind.Battleship,
            ShipKind.Carrier,
        };
        var orientations = new[]
        {
            ShipOrientation.Horizontal,
            ShipOrientation.Vertical,
            ShipOrientation.Horizontal,
            ShipOrientation.Vertical,
            ShipOrientation.Horizontal,
        };
        for (var i = 0; i < shipKinds.Length; i++)
        {
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new AddShipRequest(BoardSide.Own, shipKinds[i], orientations[i], _bowCodes[i])
            );
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/ships",
                new AddShipRequest(BoardSide.Opp, shipKinds[i], orientations[i], _bowCodes[i])
            );
        }
    }

    private async Task<Guid> CreateGame(Guid playerId)
    {
        var response = await _client.PostAsJsonAsync("/api/games", new { PlayerId = playerId, BoardSize = 10 });
        response.EnsureSuccessStatusCode();
        var createdGameLocation = response.Headers.Location;
        createdGameLocation.Should().NotBeNull();
        var gameId = ExtractIdFromLocation(createdGameLocation);
        return gameId;
    }

    private async Task<Guid> CreatePlayer(string playerUsername)
    {
        var response = await _client.PostAsJsonAsync("/api/players", new { Username = playerUsername });
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
