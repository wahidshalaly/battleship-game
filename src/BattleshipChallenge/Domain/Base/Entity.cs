namespace BattleshipChallenge.Domain.Base;

public class Entity<T>
{
    public T Id { get; protected init; }
}
