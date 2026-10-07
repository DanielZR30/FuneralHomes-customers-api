namespace FH.Shared.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;

    protected BaseEntity()
    {
    }

    protected BaseEntity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default) || EqualityComparer<TId>.Default.Equals(other.Id, default))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.Equals(Id, default)
            ? base.GetHashCode()
            : Id!.GetHashCode();
    }
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() : base(Guid.NewGuid())
    {
    }

    protected BaseEntity(Guid id) : base(id)
    {
    }
}

public abstract class AggregateRoot<TId> : BaseEntity<TId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot() : base(Guid.NewGuid())
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
