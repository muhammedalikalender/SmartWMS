namespace SmartWMS.Application.Features.Anomaly.Handlers;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartWMS.Application.Common.Models;
using SmartWMS.Application.Features.Anomaly.Orchestrator;
using SmartWMS.Domain.Events;
using SmartWMS.Domain.Common;

// 1. ADAPTER: STABILITY TRIGGER
public class ShelfStabilityTriggerHandler : INotificationHandler<DomainEventNotification<ShelfStabilityChangedDomainEvent>>
{
    private readonly IAnomalyPipelineDispatcher _dispatcher;

    public ShelfStabilityTriggerHandler(IAnomalyPipelineDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(DomainEventNotification<ShelfStabilityChangedDomainEvent> notification, CancellationToken cancellationToken)
    {
        // Staff-Level Note: Handler sadece bir adaptördür. 
        // Hiçbir Business Logic barındırmaz, sadece dispatcher'ı tetikler.
        await _dispatcher.DispatchAsync(notification.DomainEvent, cancellationToken);
    }
}

// 2. ADAPTER: ITEM ADDED TRIGGER
public class ItemAddedTriggerHandler : INotificationHandler<DomainEventNotification<ItemAddedDomainEvent>>
{
    private readonly IAnomalyPipelineDispatcher _dispatcher;

    public ItemAddedTriggerHandler(IAnomalyPipelineDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(DomainEventNotification<ItemAddedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(notification.DomainEvent, cancellationToken);
    }
}

// 3. ADAPTER: ITEM REMOVED TRIGGER
public class ItemRemovedTriggerHandler : INotificationHandler<DomainEventNotification<ItemRemovedDomainEvent>>
{
    private readonly IAnomalyPipelineDispatcher _dispatcher;

    public ItemRemovedTriggerHandler(IAnomalyPipelineDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(DomainEventNotification<ItemRemovedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(notification.DomainEvent, cancellationToken);
    }
}
