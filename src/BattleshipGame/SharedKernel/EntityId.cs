namespace BattleshipGame.SharedKernel;

public record EntityId(Guid Value)
{
    public static implicit operator Guid(EntityId id) => id.Value;

    public static implicit operator EntityId(Guid value) => new(value);

    public static TId CreateFromValue<TId>(Guid value)
        where TId : EntityId => (TId)Activator.CreateInstance(typeof(TId), value)!;
}
