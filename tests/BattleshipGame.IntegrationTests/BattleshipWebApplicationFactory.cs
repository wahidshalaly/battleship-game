using BattleshipGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipGame.IntegrationTests;

internal sealed class BattleshipWebApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
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

            // Replace real JWT bearer validation with the test scheme so integration tests
            // authenticate via a header instead of a live identity provider. Registering it as
            // the default scheme makes the API's fallback "require authenticated user" policy
            // resolve against it.
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { }
                );
        });

        // The API now validates Authentication/Keycloak options at startup (ValidateOnStart).
        // These game-mechanics tests use TestAuthHandler, so no live Keycloak is contacted — but
        // the options must still be present and non-empty. Supply placeholder values.
        builder.UseSetting("Authentication:Authority", "http://localhost/realms/test");
        builder.UseSetting("Authentication:Audience", "account");
        builder.UseSetting("Authentication:RequireHttpsMetadata", "false");
        builder.UseSetting("Keycloak:BaseUrl", "http://localhost");
        builder.UseSetting("Keycloak:Realm", "test");
        builder.UseSetting("Keycloak:ClientId", "test-client");
        builder.UseSetting("Keycloak:ClientSecret", "test-secret");
        builder.UseSetting("Keycloak:AdminUsername", "admin");
        builder.UseSetting("Keycloak:AdminPassword", "admin");

        builder.UseEnvironment("Test");
    }
}
