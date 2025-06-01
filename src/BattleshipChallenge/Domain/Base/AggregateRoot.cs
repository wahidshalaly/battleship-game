namespace BattleshipChallenge.Domain.Base;

internal abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull { }
