namespace SmartWMS.Domain.Common;

using System.Collections.Generic;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
