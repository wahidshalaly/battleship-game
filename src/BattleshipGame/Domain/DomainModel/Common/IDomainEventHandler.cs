namespace BattleshipGame.Domain.DomainModel.Common;

public interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    Task HandleAsync(T evt);
}
