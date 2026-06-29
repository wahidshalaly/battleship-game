using BattleshipGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Starts a real PostgreSQL container once per test class (IClassFixture),
/// runs migrations, and provides DbContext and WebApplicationFactory instances.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("battleship_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public BattleshipGameDbContext CreateDbContext() =>
        new(
            new DbContextOptionsBuilder<BattleshipGameDbContext>()
                .UseNpgsql(ConnectionString)
                .Options
        );
}
