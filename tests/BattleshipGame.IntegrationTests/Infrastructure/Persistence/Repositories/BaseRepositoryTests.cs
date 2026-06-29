using BattleshipGame.Infrastructure.Persistence;

namespace BattleshipGame.IntegrationTests.Infrastructure.Persistence.Repositories;

public abstract class BaseRepositoryTests(PostgresFixture postgres) : IAsyncLifetime
{
    protected readonly PostgresFixture _postgres = postgres;
    protected BattleshipGameDbContext _dbcontext = null!;

    /// <summary>
    /// Initialize a new DbContext for each test to ensure a clean slate and avoid cross-test contamination.
    /// </summary>
    /// <returns></returns>
    public virtual Task InitializeAsync()
    {
        _dbcontext = _postgres.CreateDbContext();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispose of the DbContext after each test to free resources and ensure no lingering connections.
    /// </summary>
    /// <returns></returns>
    public virtual Task DisposeAsync() => _dbcontext.DisposeAsync().AsTask();

    // Simulates the UnitOfWorkBehavior that runs in production via the MediatR pipeline.
    // Direct repository tests bypass MediatR, so saves must be committed explicitly.
    protected Task CommitAsync() => _dbcontext.SaveChangesAsync();
}
