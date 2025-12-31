namespace BattleshipGame.Domain.SharedKernel;

public interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    Task HandleAsync(T evt);
}
