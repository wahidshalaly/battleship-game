using System.Net.Http.Headers;
using System.Net.Http.Json;
using BattleshipGame.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// End-to-end tests that exercise the full auth façade against a real Keycloak container:
/// register → sign-in → /me → create game → /games/active → refresh → logout.
/// No <see cref="TestAuthHandler"/> shortcut — tokens from Keycloak are validated by
/// the real JWT bearer middleware.
/// </summary>
[Collection(AuthIntegrationCollection.Name)]
public class AuthApiTests(PostgresFixture postgres, KeycloakFixture keycloak) : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        var opts = keycloak.Options;
        _factory = new BattleshipAuthWebApplicationFactory(
            postgres.ConnectionString,
            $"{opts.BaseUrl}/realms/{opts.Realm}",
            opts.BaseUrl,
            opts.Realm,
            opts.ClientId,
            opts.ClientSecret,
            opts.AdminUsername,
            opts.AdminPassword
        );
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return _factory.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task Register_SignIn_Me_ActiveGame_Refresh_Logout_FullJourney()
    {
        var username = $"u_{Guid.NewGuid():N}"[..20];
        var email = $"{username}@battleship.test";
        const string password = "P@ssword123!";

        // 1. Register — creates identity + game profile in one call
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                Username = username,
                Email = email,
                Password = password,
            }
        );
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        registerResult.Should().NotBeNull();
        registerResult!.AccessToken.Should().NotBeNullOrEmpty();
        registerResult.RefreshToken.Should().NotBeNullOrEmpty();

        // 2. GET /players/me — profile must exist after registration
        using var authedClient = BearerClient(registerResult.AccessToken);
        var meResponse = await authedClient.GetAsync("/api/players/me");
        meResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var me = await meResponse.Content.ReadFromJsonAsync<PlayerMeResponse>();
        me!.Username.Should().Be(username);

        // 3. GET /games/active — should be 204 (no active game yet)
        var activeResponse = await authedClient.GetAsync("/api/games/active");
        activeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 4. Create a game
        var createGameResponse = await authedClient.PostAsJsonAsync(
            "/api/games",
            new { BoardSize = 10, OpponentStrategy = 0 }
        );
        createGameResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 5. GET /games/active — should now return the created game
        var activeAfterCreate = await authedClient.GetAsync("/api/games/active");
        activeAfterCreate.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 6. Sign in — independent session
        var signinResponse = await _client.PostAsJsonAsync(
            "/api/auth/signin",
            new { Username = username, Password = password }
        );
        signinResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var signinResult = await signinResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        signinResult!.RefreshToken.Should().NotBeNullOrEmpty();

        // 7. Refresh — obtain a new token pair
        var refreshResponse = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { RefreshToken = signinResult.RefreshToken }
        );
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();

        // 8. Logout — invalidates the refresh token
        var logoutResponse = await _client.PostAsJsonAsync(
            "/api/auth/logout",
            new { RefreshToken = refreshResult.RefreshToken }
        );
        logoutResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 9. Refresh after logout should fail
        var refreshAfterLogout = await _client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { RefreshToken = refreshResult.RefreshToken }
        );
        refreshAfterLogout.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WhenUsernameAlreadyExists_Returns409()
    {
        var username = $"u_{Guid.NewGuid():N}"[..20];
        var registerBody = new
        {
            Username = username,
            Email = $"{username}@battleship.test",
            Password = "P@ssword123!",
        };

        var first = await _client.PostAsJsonAsync("/api/auth/register", registerBody);
        first.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // Second registration with the same username — Keycloak returns 409
        var second = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerBody with
            {
                Email = $"other_{username}@battleship.test",
            }
        );
        second.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SignIn_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/signin",
            new { Username = "nobody", Password = "wrong" }
        );
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnonymousRequest_Returns401()
    {
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private HttpClient BearerClient(string accessToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        return client;
    }

    private sealed record PlayerMeResponse(
        Guid Id,
        string Username,
        Guid? ActiveGameId,
        int TotalGamesPlayed
    );
}
