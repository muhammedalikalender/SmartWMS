namespace SmartWMS.Infrastructure.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Common.Models;
using SmartWMS.Domain.Common;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _mediator;

    public DomainEventDispatcher(IPublisher mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAndClearEvents(IEnumerable<AggregateRoot> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                // MediatR notification tipine dinamik dönüştürerek fırlatıyoruz
                // Örn: var notification = new DomainEventNotification<ItemAddedDomainEvent>(domainEvent);
                
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

                await _mediator.Publish(notification);
            }
        }
    }
}
