using BattleshipGame.Application.Interfaces.Persistence;

namespace BattleshipGame.Infrastructure.Persistence;

internal class UnitOfWork(BattleshipGameDbContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
