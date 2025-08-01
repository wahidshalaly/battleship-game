namespace BattleshipGame.Domain.DomainModel.Common;

public record EntityId(Guid Value)
{
    public static implicit operator Guid(EntityId id) => id.Value;

    /// <summary>
    /// Creates a new instance of the specified EntityId type with a new GUID
    /// </summary>
    public static TId New<TId>()
        where TId : EntityId
    {
        return CreateInstance<TId>(Guid.NewGuid());
    }

    /// <summary>
    /// Creates an instance of the specified EntityId type from a GUID
    /// </summary>
    public static TId FromGuid<TId>(Guid guid)
        where TId : EntityId
    {
        return CreateInstance<TId>(guid);
    }

    private static TId CreateInstance<TId>(Guid guid)
        where TId : EntityId
    {
        // Get the constructor that takes a Guid parameter
        var constructor =
            typeof(TId).GetConstructor([typeof(Guid)])
            ?? throw new InvalidOperationException(
                $"Type {typeof(TId).Name} must have a constructor that accepts a Guid parameter."
            );

        return (TId)constructor.Invoke([guid]);
    }
}
