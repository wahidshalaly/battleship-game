namespace BattleshipGame.Domain.Common;

public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected init; }

    public override bool Equals(object obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Equals(Id, default(TId)) || Equals(other.Id, default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
