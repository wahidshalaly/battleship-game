using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BattleshipGame.Infrastructure.Identity;
using Testcontainers.Keycloak;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Starts a real Keycloak container and configures the battleship realm via the Admin REST API.
/// The client secret is read back from Keycloak after client creation so the test options always
/// match the secret Keycloak actually assigned.
/// </summary>
public sealed class KeycloakFixture : IAsyncLifetime
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin";
    private const string ClientId = "battleship-api";
    private const string Realm = "battleship";

    private readonly KeycloakContainer _container = new KeycloakBuilder(
        "quay.io/keycloak/keycloak:26.1"
    ).Build();

    public KeycloakOptions Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var baseUrl = _container.GetBaseAddress().TrimEnd('/');
        var clientSecret = await SetupRealmAsync(baseUrl);

        Options = new KeycloakOptions
        {
            BaseUrl = baseUrl,
            Realm = Realm,
            ClientId = ClientId,
            ClientSecret = clientSecret,
            AdminUsername = AdminUsername,
            AdminPassword = AdminPassword,
        };
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private static async Task<string> SetupRealmAsync(string baseUrl)
    {
        using var http = new HttpClient();

        // Obtain admin token from the master realm.
        var tokenResp = await http.PostAsync(
            $"{baseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli",
                    ["username"] = AdminUsername,
                    ["password"] = AdminPassword,
                }
            )
        );
        tokenResp.EnsureSuccessStatusCode();
        var tokenDoc = await tokenResp.Content.ReadFromJsonAsync<JsonElement>();
        var adminToken = tokenDoc.GetProperty("access_token").GetString()!;

        // Create the battleship realm.
        var createRealmResp = await http.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/admin/realms")
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
                Content = JsonContent.Create(new { realm = Realm, enabled = true }),
            }
        );
        createRealmResp.EnsureSuccessStatusCode();

        // Disable all realm-level default required actions so new users can sign in immediately
        // without needing to complete VERIFY_EMAIL / UPDATE_PASSWORD / UPDATE_PROFILE flows.
        // Uses JsonNode so the full action representation is preserved when PUT back.
        await DisableDefaultRequiredActionsAsync(http, baseUrl, adminToken);

        // Create the battleship-api confidential client with Direct Access Grants (ROPC).
        // Omit "secret" here; read the generated secret back below.
        var createClientResp = await http.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/admin/realms/{Realm}/clients")
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
                Content = JsonContent.Create(
                    new
                    {
                        clientId = ClientId,
                        enabled = true,
                        publicClient = false,
                        directAccessGrantsEnabled = true,
                        standardFlowEnabled = false,
                        serviceAccountsEnabled = false,
                    }
                ),
            }
        );
        createClientResp.EnsureSuccessStatusCode();

        // The Location header contains the client UUID.
        var clientUuid = createClientResp.Headers.Location!.Segments[^1].TrimEnd('/');

        // Read back the auto-generated client secret so Options.ClientSecret always matches.
        var getSecretResp = await http.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/admin/realms/{Realm}/clients/{clientUuid}/client-secret"
            )
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
            }
        );
        getSecretResp.EnsureSuccessStatusCode();
        var secretDoc = await getSecretResp.Content.ReadFromJsonAsync<JsonElement>();
        return secretDoc.GetProperty("value").GetString()!;
    }

    private static async Task DisableDefaultRequiredActionsAsync(
        HttpClient http,
        string baseUrl,
        string adminToken
    )
    {
        var listResp = await http.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/admin/realms/{Realm}/authentication/required-actions"
            )
            {
                Headers = { { "Authorization", $"Bearer {adminToken}" } },
            }
        );
        listResp.EnsureSuccessStatusCode();

        // Parse as JsonArray so we can modify nodes in-place and round-trip correctly.
        var json = await listResp.Content.ReadAsStringAsync();
        var actions = JsonNode.Parse(json)?.AsArray() ?? [];

        foreach (var actionNode in actions)
        {
            if (actionNode?["defaultAction"]?.GetValue<bool>() != true)
                continue;

            var alias = actionNode["alias"]!.GetValue<string>();
            actionNode["defaultAction"] = false;

            var putResp = await http.SendAsync(
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{baseUrl}/admin/realms/{Realm}/authentication/required-actions/{Uri.EscapeDataString(alias)}"
                )
                {
                    Headers = { { "Authorization", $"Bearer {adminToken}" } },
                    Content = new StringContent(
                        actionNode.ToJsonString(),
                        Encoding.UTF8,
                        "application/json"
                    ),
                }
            );
            putResp.EnsureSuccessStatusCode();
        }
    }
}

[CollectionDefinition(Name)]
public sealed class AuthIntegrationCollection
    : ICollectionFixture<PostgresFixture>,
        ICollectionFixture<KeycloakFixture>
{
    public const string Name = "AuthIntegration";
}
