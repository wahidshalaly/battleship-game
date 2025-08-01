namespace BattleshipGame.Domain.DomainModel.Common;

public abstract class Entity<TId>
    where TId : EntityId
{
    protected Entity()
    {
        Id = EntityId.New<TId>();
    }

    protected Entity(Guid id)
    {
        Id = EntityId.FromGuid<TId>(id);
    }

    public TId Id { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
