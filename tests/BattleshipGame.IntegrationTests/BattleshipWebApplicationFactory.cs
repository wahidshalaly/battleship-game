using BattleshipGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

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
        });

        builder.UseEnvironment("Test");
    }
}
