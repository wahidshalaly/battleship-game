using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Common.Security;
using Microsoft.Extensions.Options;

namespace BattleshipGame.Infrastructure.Identity;

/// <summary>
/// Proxies Keycloak's Admin REST API and token endpoint to provide register/signin/refresh/logout.
/// </summary>
public sealed class KeycloakIdentityProvider(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options
) : IIdentityProvider
{
    private readonly KeycloakOptions _opts = options.Value;

    public async Task<(IdentityTokens Tokens, string Subject)> RegisterAsync(
        string username,
        string email,
        string password,
        CancellationToken ct
    )
    {
        var adminToken = await GetAdminTokenAsync(ct);

        // Create the user (no credentials inline — password is set via reset-password below).
        // Keycloak 26 treats inline credentials as temporary even when temporary:false is set,
        // which blocks ROPC with "Account is not fully set up".
        var createResponse = await httpClient.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Post,
                $"{_opts.BaseUrl}/admin/realms/{_opts.Realm}/users"
            )
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
                Content = JsonContent.Create(
                    new
                    {
                        username,
                        email,
                        emailVerified = true,
                        enabled = true,
                        // Keycloak 26 Declarative User Profile requires firstName + lastName;
                        // omitting them auto-applies UPDATE_PROFILE and blocks ROPC.
                        firstName = username,
                        lastName = username,
                        requiredActions = Array.Empty<string>(),
                    }
                ),
            },
            ct
        );

        if (createResponse.StatusCode == HttpStatusCode.Conflict)
            throw new IdentityConflictException(
                $"A user with username '{username}' or email '{email}' already exists."
            );

        createResponse.EnsureSuccessStatusCode();

        // The Location header contains the new user's URL; the last segment is the Keycloak user
        // ID, which Keycloak uses as the JWT 'sub' claim.
        var location =
            createResponse.Headers.Location
            ?? throw new InvalidOperationException("Keycloak did not return a user Location.");
        var subject = location.Segments[^1].TrimEnd('/');

        // Set the password via the dedicated endpoint with temporary:false so no required action
        // is applied and the user can sign in immediately via ROPC.
        var setPasswordResponse = await httpClient.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Put,
                $"{_opts.BaseUrl}/admin/realms/{_opts.Realm}/users/{subject}/reset-password"
            )
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
                Content = JsonContent.Create(
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false,
                    }
                ),
            },
            ct
        );
        setPasswordResponse.EnsureSuccessStatusCode();

        var tokens = await SignInAsync(username, password, ct);
        return (tokens, subject);
    }

    public async Task<IdentityTokens> SignInAsync(
        string username,
        string password,
        CancellationToken ct
    )
    {
        var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/realms/{_opts.Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = _opts.ClientId,
                    ["client_secret"] = _opts.ClientSecret,
                    ["username"] = username,
                    ["password"] = password,
                }
            ),
            ct
        );

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidCredentialsException();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Keycloak ROPC failed {(int)response.StatusCode}: {body}"
            );
        }

        return await ReadTokensAsync(response, ct);
    }

    public async Task<IdentityTokens> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/realms/{_opts.Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = _opts.ClientId,
                    ["client_secret"] = _opts.ClientSecret,
                    ["refresh_token"] = refreshToken,
                }
            ),
            ct
        );

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            throw new InvalidCredentialsException();

        response.EnsureSuccessStatusCode();
        return await ReadTokensAsync(response, ct);
    }

    public async Task SignOutAsync(string refreshToken, CancellationToken ct)
    {
        var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/realms/{_opts.Realm}/protocol/openid-connect/logout",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _opts.ClientId,
                    ["client_secret"] = _opts.ClientSecret,
                    ["refresh_token"] = refreshToken,
                }
            ),
            ct
        );

        // 400/404 on logout means the token was already expired/invalid — treat as success.
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        // Use the Keycloak master realm admin-cli client with admin credentials.
        // This is the standard way to obtain admin tokens without service-account setup.
        var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli",
                    ["username"] = _opts.AdminUsername,
                    ["password"] = _opts.AdminPassword,
                }
            ),
            ct
        );
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(ct);
        return result!.AccessToken;
    }

    private static async Task<IdentityTokens> ReadTokensAsync(
        HttpResponseMessage response,
        CancellationToken ct
    )
    {
        var result = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(ct);
        return new IdentityTokens(result!.AccessToken, result.RefreshToken, result.ExpiresIn);
    }

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );
}
