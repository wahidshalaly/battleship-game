namespace BattleshipGame.SharedKernel;

public abstract class Entity<TId>
    where TId : EntityId
{
    protected Entity()
    {
        Id = EntityId.CreateFromValue<TId>(Guid.NewGuid());
    }

    protected Entity(Guid id)
    {
        Id = EntityId.CreateFromValue<TId>(id);
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
