using BattleshipGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Starts a real PostgreSQL container once for the whole test suite (ICollectionFixture),
/// runs migrations, and provides DbContext instances. Tests isolate themselves via unique
/// identifiers (GUID-based ids and username suffixes) rather than a clean database, so a
/// single shared container serves every test class without cross-test interference.
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

/// <summary>
/// Binds <see cref="PostgresFixture"/> to a single xUnit collection so one postgres:17
/// container (and one migration run) is shared by every test class in the suite.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
