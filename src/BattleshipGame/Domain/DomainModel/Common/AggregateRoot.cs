namespace BattleshipGame.Domain.DomainModel.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : EntityId;
