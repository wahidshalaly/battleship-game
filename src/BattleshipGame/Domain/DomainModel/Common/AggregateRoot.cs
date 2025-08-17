namespace BattleshipGame.Domain.DomainModel.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : EntityId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance with a new identifier.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.ToList().AsReadOnly();

    /// <summary>
    /// Adds a domain event to be published.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
