using System.Threading.Tasks;
using BattleshipGame.Domain.Events;

namespace BattleshipGame.Domain.EventHandlers;

internal interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    Task HandleAsync(T evt);
}
