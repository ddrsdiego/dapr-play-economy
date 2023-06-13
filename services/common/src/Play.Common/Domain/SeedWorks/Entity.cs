namespace Play.Common.Domain.SeedWorks;

using System;
using System.Collections.Generic;

public interface INotification
{
}

public abstract class Entity
{
    private List<INotification> _notifications;

    protected Entity()
        : this(Guid.NewGuid().ToString())
    {
    }

    protected Entity(string id)
    {
        Id = id;
    }

    public virtual string Id { get; protected set; }

    public IReadOnlyCollection<INotification> DomainEvents => _notifications?.AsReadOnly();

    protected void AddNotification(INotification eventItem)
    {
        _notifications ??= new List<INotification>();
        _notifications.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _notifications?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _notifications?.Clear();
    }
}