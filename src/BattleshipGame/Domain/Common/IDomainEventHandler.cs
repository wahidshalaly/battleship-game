using System.Threading.Tasks;

namespace BattleshipGame.Domain.Common;

public interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    Task HandleAsync(T evt);
}
