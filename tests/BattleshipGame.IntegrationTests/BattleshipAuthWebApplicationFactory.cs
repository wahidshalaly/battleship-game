using BattleshipGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// WebApplicationFactory for e2e auth tests that use real JWT tokens from Keycloak.
/// Unlike <see cref="BattleshipWebApplicationFactory"/>, this does NOT replace JWT bearer
/// with the test scheme — tokens from Keycloak are validated against the live container.
/// </summary>
internal sealed class BattleshipAuthWebApplicationFactory(
    string connectionString,
    string keycloakAuthority,
    string keycloakBaseUrl,
    string keycloakRealm,
    string keycloakClientId,
    string keycloakClientSecret,
    string keycloakAdminUsername,
    string keycloakAdminPassword,
    FakeLlmChat? llmChat = null
) : WebApplicationFactory<Program>
{
    // Runs before the first factory instance boots the API, which reads OPENAI_* at startup.
    static BattleshipAuthWebApplicationFactory() => TestOpenAiSettings.EnsureEnvironment();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<BattleshipGameDbContext>)
            );
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<BattleshipGameDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // Never reach a real LLM: the LLM opponent talks to a scripted fake.
            (llmChat ?? new FakeLlmChat()).Register(services);
        });

        // Point auth and Keycloak config at the test containers.
        builder.UseSetting("Authentication:Authority", keycloakAuthority);
        builder.UseSetting("Authentication:Audience", "account");
        builder.UseSetting("Authentication:RequireHttpsMetadata", "false");
        builder.UseSetting("Keycloak:BaseUrl", keycloakBaseUrl);
        builder.UseSetting("Keycloak:Realm", keycloakRealm);
        builder.UseSetting("Keycloak:ClientId", keycloakClientId);
        builder.UseSetting("Keycloak:ClientSecret", keycloakClientSecret);
        builder.UseSetting("Keycloak:AdminUsername", keycloakAdminUsername);
        builder.UseSetting("Keycloak:AdminPassword", keycloakAdminPassword);

        // A failing AI opponent is retried with real (2s+) backoff before falling back; one retry
        // keeps those tests fast while still exercising the retry path.
        builder.UseSetting("Resilience:AiOpponent:MaxRetryAttempts", "1");

        builder.UseEnvironment("Test");
    }
}
