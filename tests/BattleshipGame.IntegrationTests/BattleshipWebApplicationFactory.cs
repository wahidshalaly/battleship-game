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

        builder.UseEnvironment("Test");
    }
}
