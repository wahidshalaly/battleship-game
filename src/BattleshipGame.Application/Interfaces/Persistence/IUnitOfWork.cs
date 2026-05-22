namespace BattleshipGame.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
}
