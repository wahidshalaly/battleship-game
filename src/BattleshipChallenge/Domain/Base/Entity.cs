namespace BattleshipChallenge.Domain.Base;

internal abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected init; }

    // Override Equals
    public override bool Equals(object obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // If either is transient (unpersisted), they’re not equal
        if (Equals(Id, default(TId)) || Equals(other.Id, default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    // Override GetHashCode
    public override int GetHashCode() => Id.GetHashCode();
}
